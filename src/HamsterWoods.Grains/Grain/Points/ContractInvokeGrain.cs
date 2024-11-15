using AElf;
using AElf.Client.Dto;
using AElf.Client.Service;
using AElf.Types;
using Google.Protobuf;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.Enums;
using HamsterWoods.Grains.Grain.Options;
using HamsterWoods.Grains.State.Points;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Volo.Abp.ObjectMapping;
using EnumConverter = HamsterWoods.Commons.EnumConverter;

namespace HamsterWoods.Grains.Grain.Points;

public class ContractInvokeGrain : Grain<ContractInvokeState>, IContractInvokeGrain
{
    private readonly IObjectMapper _objectMapper;
    private readonly ChainOptions _options;
    private readonly ILogger<ContractInvokeGrain> _logger;
    private readonly IBlockchainClientFactory<AElfClient> _blockchainClientFactory;
    private readonly PointsOptions _pointsOptions;

    public ContractInvokeGrain(IObjectMapper objectMapper, ILogger<ContractInvokeGrain> logger,
        IBlockchainClientFactory<AElfClient> blockchainClientFactory,
        IOptionsSnapshot<ChainOptions> options,
        IOptionsSnapshot<PointsOptions> pointsOptions)
    {
        _objectMapper = objectMapper;
        _logger = logger;
        _blockchainClientFactory = blockchainClientFactory;
        _options = options.Value;
        _pointsOptions = pointsOptions.Value;
    }

    public override async Task OnActivateAsync()
    {
        await ReadStateAsync();
        await base.OnActivateAsync();
    }

    public override async Task OnDeactivateAsync()
    {
        await WriteStateAsync();
        await base.OnDeactivateAsync();
    }

    public async Task<GrainResultDto<ContractInvokeGrainDto>> CreateAsync(ContractInvokeGrainDto input)
    {
        if (State.BizId != null && State.BizId.Equals(input.BizId))
        {
            _logger.LogInformation(
                "CreateAsync contract invoke repeated bizId {bizId} ", State.BizId);
            return OfContractInvokeGrainResultDto(true, CommonConstant.TradeRepeated);
        }

        State = _objectMapper.Map<ContractInvokeGrainDto, ContractInvokeState>(input);
        if (State.Id.IsNullOrEmpty())
        {
            State.Id = input.BizId;
        }

        State.Status = ContractInvokeStatus.ToBeCreated.ToString();
        State.CreateTime = DateTime.UtcNow;
        State.UpdateTime = DateTime.UtcNow;

        await WriteStateAsync();

        _logger.LogInformation(
            "CreateAsync Contract bizId {bizId} created.", State.BizId);

        return OfContractInvokeGrainResultDto(true);
    }

    public async Task<GrainResultDto<ContractInvokeGrainDto>> ExecuteJobAsync(ContractInvokeGrainDto input)
    {
        //State = _objectMapper.Map<ContractInvokeGrainDto, ContractInvokeState>(input);
        //if the data in the grain memory has reached the final state then idempotent return
        if (IsFinalStatus(State.Status))
        {
            return OfContractInvokeGrainResultDto(true);
        }

        var status = EnumConverter.ConvertToEnum<ContractInvokeStatus>(State.Status);

        try
        {
            switch (status)
            {
                case ContractInvokeStatus.ToBeCreated:
                    await HandleCreatedAsync();
                    break;
                case ContractInvokeStatus.Pending:
                    await HandlePendingAsync();
                    break;
                case ContractInvokeStatus.Failed:
                    await HandleFailedAsync();
                    break;
            }

            return OfContractInvokeGrainResultDto(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An error occurred during job execution and will be retried bizId:{bizId} txHash: {TxHash}",
                State.BizId, State.TransactionId);
            return OfContractInvokeGrainResultDto(false);
        }
    }

    public async Task<GrainResultDto<ContractInvokeGrainDto>> ReExecuteAsync()
    {
        if (State.Id.IsNullOrEmpty())
        {
            return OfContractInvokeGrainResultDto(false, "Contract not exist.");
        }

        if (State.Status != ContractInvokeStatus.FinalFailed.ToString())
        {
            return OfContractInvokeGrainResultDto(false,
                $"Contract status is not FinalFailed, status:{State.Status ?? "-"}");
        }

        State.Status = ContractInvokeStatus.ToBeCreated.ToString();
        State.UpdateTime = DateTime.UtcNow;

        await WriteStateAsync();
        return OfContractInvokeGrainResultDto(true);
    }

    private async Task HandleCreatedAsync()
    {
        //To Generate RawTransaction and Send Transaction
        if (!_options.ChainInfos.TryGetValue(State.ChainId, out var chainInfo))
        {
            _logger.LogError("ChainOptions chainId:{chainId} has no chain info.", State.ChainId);
            return;
        }

        var client = _blockchainClientFactory.GetClient(State.ChainId);
        var rawTxResult = await GenerateRawTransaction(State.ContractMethod,
            State.Param, State.ChainId, State.ContractAddress);
        //save txId
        var oriStatus = State.Status;
        var signedTransaction = rawTxResult.Item2;
        var txId = signedTransaction.GetHash().ToHex();
        State.RefBlockNumber = rawTxResult.Item1;
        State.Sender = client.GetAddressFromPrivateKey(chainInfo.PrivateKey);
        State.TransactionId = txId;
        State.Status = ContractInvokeStatus.Pending.ToString();
        //Send Transaction
        await SendTransactionAsync(State.ChainId, signedTransaction);

        _logger.LogInformation(
            "HandleCreatedAsync Contract bizId {bizId} methodName:{methodName} txHash:{txHash} invoke status {oriStatus} to {status}",
            State.BizId, State.ContractMethod, State.TransactionId, oriStatus, State.Status);

        await WriteStateAsync();
    }

    private async Task HandlePendingAsync()
    {
        //To Get Transaction Result
        if (State.TransactionId.IsNullOrEmpty())
        {
            await HandleFailedAsync();
            return;
        }

        var txResult = await GetTxResultAsync(State.ChainId, State.TransactionId);
        switch (txResult.Status)
        {
            case ContractConstant.Mined:
                await HandleSuccessAsync(txResult);
                break;
            case ContractConstant.Pending:
                break;
            case ContractConstant.Notexisted:
                var client = _blockchainClientFactory.GetClient(State.ChainId);
                var status = await client.GetChainStatusAsync();
                var libHeight = status.LastIrreversibleBlockHeight;
                //check libHeight - refBlockNumber
                if (libHeight - State.RefBlockNumber > _pointsOptions.BlockPackingMaxHeightDiff)
                {
                    await UpdateFailedAsync(txResult);
                }

                break;
            default:
                await UpdateFailedAsync(txResult);
                break;
        }
    }

    private async Task HandleFailedAsync()
    {
        //To retry and send HandleCreatedAsync
        if (State.RetryCount >= _pointsOptions.MaxRetryCount)
        {
            State.Status = ContractInvokeStatus.FinalFailed.ToString();
        }
        else
        {
            State.Status = ContractInvokeStatus.ToBeCreated.ToString();
            State.RetryCount += 1;
        }

        _logger.LogInformation(
            "HandleFailedAsync Contract bizId {bizId} methodName:{methodName} txHash:{txHash} invoke status to {status}, retryCount:{retryCount}",
            State.BizId, State.ContractMethod, State.TransactionId, State.Status, State.RetryCount);
        await WriteStateAsync();
    }


    private async Task HandleSuccessAsync(TransactionResultDto txResult)
    {
        var oriStatus = State.Status;
        State.BlockHeight = txResult.BlockNumber;
        State.Status = ContractInvokeStatus.Success.ToString();
        _logger.LogInformation(
            "HandlePendingAsync Contract bizId {bizId} methodName:{methodName} txHash:{txHash} invoke status {oriStatus} to {status}",
            State.BizId, State.ContractMethod, State.TransactionId, oriStatus, State.Status);
        await WriteStateAsync();
    }

    private async Task UpdateFailedAsync(TransactionResultDto txResult)
    {
        var oriStatus = State.Status;
        State.Status = ContractInvokeStatus.Failed.ToString();
        State.TransactionStatus = txResult.Status;
        // When Transaction status is not mined or pending, Transaction is judged to be failed.
        State.Message = $"Transaction failed, status: {State.Status}. error: {txResult.Error}";
        _logger.LogWarning(
            "TransactionFailedAsync Contract bizId {bizId} methodName:{methodName} txHash:{txHash} invoke status {oriStatus} to {status}",
            State.BizId, State.ContractMethod, State.TransactionId, oriStatus, State.Status);

        await WriteStateAsync();
    }

    private async Task SendTransactionAsync(string chainId, Transaction signedTransaction)
    {
        try
        {
            var client = _blockchainClientFactory.GetClient(chainId);
            await client.SendTransactionAsync(new SendTransactionInput
                { RawTransaction = signedTransaction.ToByteArray().ToHex() });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SendTransaction error, txId:{txId}", signedTransaction.GetHash().ToHex());
        }
    }

    private async Task<(long, Transaction)> GenerateRawTransaction(string methodName, string param, string chainId,
        string contractAddress)
    {
        if (!_options.ChainInfos.TryGetValue(chainId, out var chainInfo)) return (0, null);

        var client = _blockchainClientFactory.GetClient(chainId);
        var status = await client.GetChainStatusAsync();
        var height = status.BestChainHeight;
        var blockHash = status.BestChainHash;
        var from = client.GetAddressFromPrivateKey(chainInfo.PrivateKey);
        var transaction = new Transaction
        {
            From = Address.FromBase58(from),
            To = Address.FromBase58(contractAddress),
            MethodName = methodName,
            Params = ByteString.FromBase64(param),
            RefBlockNumber = height,
            RefBlockPrefix = ByteString.CopyFrom(Hash.LoadFromHex(blockHash).Value.Take(4).ToArray())
        };
        //transaction
        return (height, client.SignTransaction(chainInfo.PrivateKey, transaction));
    }

    private async Task<TransactionResultDto> GetTxResultAsync(string chainId, string txId)
    {
        var client = _blockchainClientFactory.GetClient(chainId);
        return await client.GetTransactionResultAsync(txId);
    }

    private GrainResultDto<ContractInvokeGrainDto> OfContractInvokeGrainResultDto(bool success, string message = null)
    {
        return new GrainResultDto<ContractInvokeGrainDto>()
        {
            Data = _objectMapper.Map<ContractInvokeState, ContractInvokeGrainDto>(State),
            Success = success,
            Message = message
        };
    }

    private bool IsFinalStatus(string status)
    {
        return status.Equals(ContractInvokeStatus.Success.ToString())
               || status.Equals(ContractInvokeStatus.FinalFailed.ToString());
    }
}