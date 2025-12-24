using Abig2025.Models.Properties.Enums;

namespace Abig2025.Models.Properties
{
    public class FeatureDefinition
    {
        public int FeatureDefinitionId { get; set; }

        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Icon { get; set; }

        public FeatureValueType ValueType { get; set; }
        public FeatureScope Scope { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public string Group { get; set; } = string.Empty;
    }
}

