// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: awaken_token_contract.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using System.Collections.Generic;
using aelf = global::AElf.CSharp.Core;

namespace Awaken.Contracts.Token {

  #region Events
  internal partial class Transferred : aelf::IEvent<Transferred>
  {
    public global::System.Collections.Generic.IEnumerable<Transferred> GetIndexed()
    {
      return new List<Transferred>
      {
      new Transferred
      {
        From = From
      },
      new Transferred
      {
        To = To
      },
      new Transferred
      {
        Symbol = Symbol
      },
      };
    }

    public Transferred GetNonIndexed()
    {
      return new Transferred
      {
        Amount = Amount,
        Memo = Memo,
      };
    }
  }

  internal partial class Approved : aelf::IEvent<Approved>
  {
    public global::System.Collections.Generic.IEnumerable<Approved> GetIndexed()
    {
      return new List<Approved>
      {
      new Approved
      {
        Owner = Owner
      },
      new Approved
      {
        Spender = Spender
      },
      new Approved
      {
        Symbol = Symbol
      },
      };
    }

    public Approved GetNonIndexed()
    {
      return new Approved
      {
        Amount = Amount,
      };
    }
  }

  internal partial class UnApproved : aelf::IEvent<UnApproved>
  {
    public global::System.Collections.Generic.IEnumerable<UnApproved> GetIndexed()
    {
      return new List<UnApproved>
      {
      new UnApproved
      {
        Owner = Owner
      },
      new UnApproved
      {
        Spender = Spender
      },
      new UnApproved
      {
        Symbol = Symbol
      },
      };
    }

    public UnApproved GetNonIndexed()
    {
      return new UnApproved
      {
        Amount = Amount,
      };
    }
  }

  internal partial class Burned : aelf::IEvent<Burned>
  {
    public global::System.Collections.Generic.IEnumerable<Burned> GetIndexed()
    {
      return new List<Burned>
      {
      new Burned
      {
        Burner = Burner
      },
      new Burned
      {
        Symbol = Symbol
      },
      };
    }

    public Burned GetNonIndexed()
    {
      return new Burned
      {
        Amount = Amount,
      };
    }
  }

  internal partial class TokenCreated : aelf::IEvent<TokenCreated>
  {
    public global::System.Collections.Generic.IEnumerable<TokenCreated> GetIndexed()
    {
      return new List<TokenCreated>
      {
      };
    }

    public TokenCreated GetNonIndexed()
    {
      return new TokenCreated
      {
        Symbol = Symbol,
        TokenName = TokenName,
        TotalSupply = TotalSupply,
        Decimals = Decimals,
        Issuer = Issuer,
        IsBurnable = IsBurnable,
        ExternalInfo = ExternalInfo,
      };
    }
  }

  internal partial class Issued : aelf::IEvent<Issued>
  {
    public global::System.Collections.Generic.IEnumerable<Issued> GetIndexed()
    {
      return new List<Issued>
      {
      };
    }

    public Issued GetNonIndexed()
    {
      return new Issued
      {
        Symbol = Symbol,
        Amount = Amount,
        Memo = Memo,
        To = To,
      };
    }
  }

  internal partial class ExternalInfoChanged : aelf::IEvent<ExternalInfoChanged>
  {
    public global::System.Collections.Generic.IEnumerable<ExternalInfoChanged> GetIndexed()
    {
      return new List<ExternalInfoChanged>
      {
      };
    }

    public ExternalInfoChanged GetNonIndexed()
    {
      return new ExternalInfoChanged
      {
        Symbol = Symbol,
        ExternalInfo = ExternalInfo,
      };
    }
  }

  #endregion
  internal static partial class TokenContractContainer
  {
    static readonly string __ServiceName = "TokenContract";

    #region Marshallers
    static readonly aelf::Marshaller<global::AElf.Types.Transaction> __Marshaller_aelf_Transaction = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Types.Transaction.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Standards.ACS2.ResourceInfo> __Marshaller_acs2_ResourceInfo = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Standards.ACS2.ResourceInfo.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.InitializeInput> __Marshaller_InitializeInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.InitializeInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.Empty> __Marshaller_google_protobuf_Empty = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Empty.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.CreateInput> __Marshaller_CreateInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.CreateInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.IssueInput> __Marshaller_IssueInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.IssueInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.TransferInput> __Marshaller_TransferInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.TransferInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.TransferFromInput> __Marshaller_TransferFromInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.TransferFromInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.ApproveInput> __Marshaller_ApproveInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.ApproveInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.UnApproveInput> __Marshaller_UnApproveInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.UnApproveInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.BurnInput> __Marshaller_BurnInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.BurnInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.ResetExternalInfoInput> __Marshaller_ResetExternalInfoInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.ResetExternalInfoInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Types.Address> __Marshaller_aelf_Address = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Types.Address.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.GetTokenInfoInput> __Marshaller_GetTokenInfoInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.GetTokenInfoInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.TokenInfo> __Marshaller_TokenInfo = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.TokenInfo.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.GetBalanceInput> __Marshaller_GetBalanceInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.GetBalanceInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.Balance> __Marshaller_Balance = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.Balance.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.GetBalancesInput> __Marshaller_GetBalancesInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.GetBalancesInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.Balances> __Marshaller_Balances = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.Balances.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.GetAllowanceInput> __Marshaller_GetAllowanceInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.GetAllowanceInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Token.Allowance> __Marshaller_Allowance = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Token.Allowance.Parser.ParseFrom);
    #endregion

    #region Methods
    static readonly aelf::Method<global::AElf.Types.Transaction, global::AElf.Standards.ACS2.ResourceInfo> __Method_GetResourceInfo = new aelf::Method<global::AElf.Types.Transaction, global::AElf.Standards.ACS2.ResourceInfo>(
        aelf::MethodType.View,
        __ServiceName,
        "GetResourceInfo",
        __Marshaller_aelf_Transaction,
        __Marshaller_acs2_ResourceInfo);

    static readonly aelf::Method<global::Awaken.Contracts.Token.InitializeInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Initialize = new aelf::Method<global::Awaken.Contracts.Token.InitializeInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Initialize",
        __Marshaller_InitializeInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Token.CreateInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Create = new aelf::Method<global::Awaken.Contracts.Token.CreateInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Create",
        __Marshaller_CreateInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Token.IssueInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Issue = new aelf::Method<global::Awaken.Contracts.Token.IssueInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Issue",
        __Marshaller_IssueInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Token.TransferInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Transfer = new aelf::Method<global::Awaken.Contracts.Token.TransferInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Transfer",
        __Marshaller_TransferInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Token.TransferFromInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_TransferFrom = new aelf::Method<global::Awaken.Contracts.Token.TransferFromInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "TransferFrom",
        __Marshaller_TransferFromInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Token.ApproveInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Approve = new aelf::Method<global::Awaken.Contracts.Token.ApproveInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Approve",
        __Marshaller_ApproveInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Token.UnApproveInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_UnApprove = new aelf::Method<global::Awaken.Contracts.Token.UnApproveInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "UnApprove",
        __Marshaller_UnApproveInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Token.BurnInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Burn = new aelf::Method<global::Awaken.Contracts.Token.BurnInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Burn",
        __Marshaller_BurnInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Token.ResetExternalInfoInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_ResetExternalInfo = new aelf::Method<global::Awaken.Contracts.Token.ResetExternalInfoInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "ResetExternalInfo",
        __Marshaller_ResetExternalInfoInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty> __Method_AddMinter = new aelf::Method<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "AddMinter",
        __Marshaller_aelf_Address,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty> __Method_RemoveMinter = new aelf::Method<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "RemoveMinter",
        __Marshaller_aelf_Address,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Token.GetTokenInfoInput, global::Awaken.Contracts.Token.TokenInfo> __Method_GetTokenInfo = new aelf::Method<global::Awaken.Contracts.Token.GetTokenInfoInput, global::Awaken.Contracts.Token.TokenInfo>(
        aelf::MethodType.View,
        __ServiceName,
        "GetTokenInfo",
        __Marshaller_GetTokenInfoInput,
        __Marshaller_TokenInfo);

    static readonly aelf::Method<global::Awaken.Contracts.Token.GetBalanceInput, global::Awaken.Contracts.Token.Balance> __Method_GetBalance = new aelf::Method<global::Awaken.Contracts.Token.GetBalanceInput, global::Awaken.Contracts.Token.Balance>(
        aelf::MethodType.View,
        __ServiceName,
        "GetBalance",
        __Marshaller_GetBalanceInput,
        __Marshaller_Balance);

    static readonly aelf::Method<global::Awaken.Contracts.Token.GetBalancesInput, global::Awaken.Contracts.Token.Balances> __Method_GetBalances = new aelf::Method<global::Awaken.Contracts.Token.GetBalancesInput, global::Awaken.Contracts.Token.Balances>(
        aelf::MethodType.View,
        __ServiceName,
        "GetBalances",
        __Marshaller_GetBalancesInput,
        __Marshaller_Balances);

    static readonly aelf::Method<global::Awaken.Contracts.Token.GetAllowanceInput, global::Awaken.Contracts.Token.Allowance> __Method_GetAllowance = new aelf::Method<global::Awaken.Contracts.Token.GetAllowanceInput, global::Awaken.Contracts.Token.Allowance>(
        aelf::MethodType.View,
        __ServiceName,
        "GetAllowance",
        __Marshaller_GetAllowanceInput,
        __Marshaller_Allowance);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::AElf.Types.Address> __Method_GetOwner = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::AElf.Types.Address>(
        aelf::MethodType.View,
        __ServiceName,
        "GetOwner",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_aelf_Address);

    #endregion

    #region Descriptors
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Awaken.Contracts.Token.AwakenTokenContractReflection.Descriptor.Services[0]; }
    }

    public static global::System.Collections.Generic.IReadOnlyList<global::Google.Protobuf.Reflection.ServiceDescriptor> Descriptors
    {
      get
      {
        return new global::System.Collections.Generic.List<global::Google.Protobuf.Reflection.ServiceDescriptor>()
        {
          global::AElf.Standards.ACS12.Acs12Reflection.Descriptor.Services[0],
          global::AElf.Standards.ACS2.Acs2Reflection.Descriptor.Services[0],
          global::Awaken.Contracts.Token.AwakenTokenContractReflection.Descriptor.Services[0],
        };
      }
    }
    #endregion

    public class TokenContractReferenceState : global::AElf.Sdk.CSharp.State.ContractReferenceState
    {
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::AElf.Types.Transaction, global::AElf.Standards.ACS2.ResourceInfo> GetResourceInfo { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.InitializeInput, global::Google.Protobuf.WellKnownTypes.Empty> Initialize { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.CreateInput, global::Google.Protobuf.WellKnownTypes.Empty> Create { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.IssueInput, global::Google.Protobuf.WellKnownTypes.Empty> Issue { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.TransferInput, global::Google.Protobuf.WellKnownTypes.Empty> Transfer { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.TransferFromInput, global::Google.Protobuf.WellKnownTypes.Empty> TransferFrom { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.ApproveInput, global::Google.Protobuf.WellKnownTypes.Empty> Approve { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.UnApproveInput, global::Google.Protobuf.WellKnownTypes.Empty> UnApprove { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.BurnInput, global::Google.Protobuf.WellKnownTypes.Empty> Burn { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.ResetExternalInfoInput, global::Google.Protobuf.WellKnownTypes.Empty> ResetExternalInfo { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty> AddMinter { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty> RemoveMinter { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.GetTokenInfoInput, global::Awaken.Contracts.Token.TokenInfo> GetTokenInfo { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.GetBalanceInput, global::Awaken.Contracts.Token.Balance> GetBalance { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.GetBalancesInput, global::Awaken.Contracts.Token.Balances> GetBalances { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Awaken.Contracts.Token.GetAllowanceInput, global::Awaken.Contracts.Token.Allowance> GetAllowance { get; set; }
      internal global::AElf.Sdk.CSharp.State.MethodReference<global::Google.Protobuf.WellKnownTypes.Empty, global::AElf.Types.Address> GetOwner { get; set; }
    }
  }
}
#endregion

