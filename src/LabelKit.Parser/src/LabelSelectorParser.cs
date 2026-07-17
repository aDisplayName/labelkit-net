// Copyright (c) 2024 Moritz Rinow. All rights reserved.

namespace LabelKit;

using Internal;
using Pidgin;

public static class LabelSelectorParser
{
  /// <summary>
  /// Parses the specified selector.
  /// </summary>
  /// <param name="selector">The selector.</param>
  /// <param name="matchingOptions">Matching options applied when evaluating the parsed selector.</param>
  /// <returns>Parsed selector or null if errors occured.</returns>
  public static LabelSelector? Parse(string selector, MatchingOptions? matchingOptions = null)
  {
    var result = Parsers.LabelSelectorExpressions.ParseOrThrow(selector);

    return result.Success ? LabelSelector.New(result.Value, false, matchingOptions) : null;
  }

  /// <summary>
  /// Attempts to parse the specified selector.
  /// </summary>
  /// <param name="selector">The raw selector.</param>
  /// <param name="parsed">Parsed selector.</param>
  /// <param name="matchingOptions">Matching options applied when evaluating the parsed selector.</param>
  /// <returns>Parse error if one occured.</returns>
  public static ParseError<LabelSelectorToken>? TryParse(string selector, out LabelSelector parsed, MatchingOptions? matchingOptions = null)
  {
    parsed = null!;

    var result = Parsers.LabelSelectorExpressions.ParseOrThrow(selector);

    if (!result.Success)
    {
      return result.Error;
    }

    parsed = LabelSelector.New(result.Value, false, matchingOptions);

    return null;
  }

  /// <summary>
  /// Parses the selector throwing an exception in case of an error.
  /// </summary>
  /// <param name="selector">The raw selector.</param>
  /// <param name="matchingOptions">Matching options applied when evaluating the parsed selector.</param>
  /// <returns>Parsed selector.</returns>
  /// <exception cref="LabelSelectorParserException">There was an error while parsing.</exception>
  public static LabelSelector ParseOrThrow(string selector, MatchingOptions? matchingOptions = null)
  {
    if (TryParse(selector, out var parsed, matchingOptions) is { } error)
    {
      throw new LabelSelectorParserException(error);
    }

    return parsed;
  }
}
