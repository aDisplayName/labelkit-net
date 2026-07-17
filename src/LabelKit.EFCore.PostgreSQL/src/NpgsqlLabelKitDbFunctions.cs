// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// PostgreSQL-specific database functions used by LabelKit JSONB label selectors.
/// Register via <see cref="NpgsqlLabelKitModelBuilderExtensions.ConfigureLabelKitNpgsql"/>.
/// </summary>
public static class NpgsqlLabelKitDbFunctions
{
  /// <summary>
  /// Gets a top-level JSONB property value as text.
  /// </summary>
  /// <param name="json">The JSONB column or value.</param>
  /// <param name="key">The top-level key.</param>
  /// <returns>The text value at the key, or null if missing.</returns>
  public static string? JsonbGetText(object json, string key)
    => throw new InvalidOperationException("This function is for use in EF Core LINQ queries only.");
}
