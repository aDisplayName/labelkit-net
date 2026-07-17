// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

using System.Text.RegularExpressions;

/// <summary>
/// Options for offline label matching, including regular-expression settings for Like / NotLike operators.
/// </summary>
public sealed class MatchingOptions
{
  /// <summary>
  /// Default matching options with a 200ms regex timeout and no regex flags.
  /// </summary>
  public static MatchingOptions Default { get; } = new();

  /// <summary>
  /// Maximum time allowed for a single regular-expression match attempt.
  /// </summary>
  public TimeSpan MatchTimeout { get; init; } = TimeSpan.FromMilliseconds(200);

  /// <summary>
  /// Options passed to <see cref="Regex.IsMatch(string, string, RegexOptions, TimeSpan)"/> for Like / NotLike matching.
  /// </summary>
  public RegexOptions RegexOptions { get; init; } = RegexOptions.None;
}
