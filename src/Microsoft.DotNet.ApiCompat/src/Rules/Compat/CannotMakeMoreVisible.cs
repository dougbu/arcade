// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Cci.Extensions;
using Microsoft.Cci.Extensions.CSharp;

namespace Microsoft.Cci.Differs.Rules
{
    [ExportDifferenceRule]
    internal class CannotMakeMoreVisible : CompatDifferenceRule
    {
        public override DifferenceType Diff(IDifferences differences, ITypeDefinitionMember impl, ITypeDefinitionMember contract)
        {
            if (impl == null || contract == null)
                return DifferenceType.Unknown;

            if (HasReducedVisibility(contract.Visibility, impl.Visibility))
            {
                differences.AddIncompatibleDifference(this,
                    $"Visibility of member '{impl.FullName()}' is '{impl.GetVisibilityName()}' in the {Implementation} but '{contract.GetVisibilityName()}' in the {Contract}.");
                return DifferenceType.Changed;
            }

            return DifferenceType.Unknown;
        }

        public override DifferenceType Diff(IDifferences differences, ITypeDefinition impl, ITypeDefinition contract)
        {
            if (impl == null || contract == null)
                return DifferenceType.Unknown;

            if (HasReducedVisibility(contract.GetVisibility(), impl.GetVisibility()))
            {
                differences.AddIncompatibleDifference(this,
                    $"Visibility of type '{impl.FullName()}' is '{impl.GetVisibilityName()}' in the {Implementation} but '{contract.GetVisibilityName()}' in the {Contract}.");
                return DifferenceType.Changed;
            }

            return DifferenceType.Unknown;
        }

        private bool HasReducedVisibility(TypeMemberVisibility contract, TypeMemberVisibility implementation)
        {
            if (contract == implementation)
            {
                return false;
            }

            switch (implementation)
            {
                case TypeMemberVisibility.Public:
                    // If implementation is public, contract can have any visibility.
                    return false;
                case TypeMemberVisibility.FamilyOrAssembly:
                    // protected internal reduces visibility from public.
                    return contract == TypeMemberVisibility.Public;
                case TypeMemberVisibility.Assembly: // internal
                case TypeMemberVisibility.Family: // protected
                    // internal and protected reduce visibility from all but private or private protected.
                    return contract != TypeMemberVisibility.Private && contract != TypeMemberVisibility.FamilyAndAssembly;
                case TypeMemberVisibility.FamilyAndAssembly:
                    // private protected is very restrictive; reduces visibility from all but private.
                    return contract != TypeMemberVisibility.Private;
                case TypeMemberVisibility.Private:
                    // private in the implementation always reduces visibility.
                    return true;
            }

            return false;
        }
    }
}
