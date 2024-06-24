namespace PancakeEditor.Common
{
    public enum EValidationErrorCode
    {
        NoError,
        IsNullOrEmpty,
        StartWithNumber,
        ContainsInvalidWord,
        ContainsWhiteSpace,
        IsDuplicate,
    }
}