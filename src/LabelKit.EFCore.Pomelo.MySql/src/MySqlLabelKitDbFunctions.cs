// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

/// <summary>
/// MySQL-specific database functions used by LabelKit JSON label selectors.
/// Register via <see cref="MySqlLabelKitModelBuilderExtensions.ConfigureLabelKitMySql"/>.
/// </summary>
public static class MySqlLabelKitDbFunctions
{
  /// <summary>
  /// Gets a JSON property value as text at the given path.
  /// </summary>
  /// <param name="json">The JSON column or value.</param>
  /// <param name="path">The JSON path (for example <c>$.label1</c>).</param>
  /// <returns>The text value at the path, or null if missing.</returns>
  public static string? JsonGetText(object json, string path)
    => throw new InvalidOperationException("This function is for use in EF Core LINQ queries only.");
}
