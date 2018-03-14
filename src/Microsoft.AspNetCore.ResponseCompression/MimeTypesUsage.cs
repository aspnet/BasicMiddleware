namespace Microsoft.AspNetCore.ResponseCompression
{
	/// <summary>
	/// Specifies how the MIME types specified in compression options should be used.
	/// </summary>
	public enum MimeTypesUsage
	{
		/// <summary>
		/// Compress only responses with specified MIME types.
		/// </summary>
		CompressSpecified,

		/// <summary>
		/// Compress responses with any MIME types except specified.
		/// </summary>
		CompressAllExceptSpecified
	}
}