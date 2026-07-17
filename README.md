# LabelKit

LabelKit is a .NET toolkit for parsing [kubernetes-like label-selectors](https://kubernetes.io/docs/concepts/overview/working-with-objects/labels/#label-selectors) and using them to build filters, including query-expressions for EFCore (PostgreSQL, MySql, SqlServer, Sqlite).
Labels are name/value pairs that can be represented as dictionaries (name->value) or simple string collections (name+delimiter+value).

- [Packages](#packages)
- [Building and CI](#building-and-ci)
  - [Continuous integration](#continuous-integration)
  - [Versioning](#versioning)
  - [Releasing to NuGet](#releasing-to-nuget)
- [Examples](#examples)
  - [EFCore PostgreSQL](#efcore-postgresql)
  - [Label-Selectors](#label-selectors)
  - [Matching Labels Offline](#matching-labels-offline)
- [Expressions (EFCore)](#expressions-efcore)
  - [Supported EFCore Providers](#supported-efcore-providers)
  - [Provided Expression-Builders](#provided-expression-builders)
  - [Filter IQueryables](#filter-iqueryables)

## Packages

The majority of packages support netstandard2.0.

The following NuGet packages are provided:

- [aDisplayName.LabelKit](https://www.nuget.org/packages/aDisplayName.LabelKit/)
  - You need to reference structured label-selectors.
- [aDisplayName.LabelKit.Parser](https://www.nuget.org/packages/aDisplayName.LabelKit.Parser/)
  - You need to parse raw label-selectors.
- [aDisplayName.LabelKit.Expressions](https://www.nuget.org/packages/aDisplayName.LabelKit.Expressions/)
  - You need to build expressions and filter queries. See [here](#expressions-efcore).
- [aDisplayName.LabelKit.EFCore.PostgreSQL](https://www.nuget.org/packages/aDisplayName.LabelKit.EFCore.PostgreSQL/)
  - You need to filter EFCore-PostgreSQL queries. See [here](#expressions-efcore).
- [aDisplayName.LabelKit.EFCore.Pomelo.MySql](https://www.nuget.org/packages/aDisplayName.LabelKit.EFCore.Pomelo.MySql/)
  - You need to filter EFCore-MySql queries. See [here](#expressions-efcore).

## Building and CI

GitHub Actions builds, tests, and packs all NuGet packages on every push and pull request to `master`. Releases are published to [nuget.org](https://www.nuget.org) via [NuGet trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) (OIDC — no long-lived API keys).

| Workflow | Trigger | Purpose |
|---|---|---|
| [CI](.github/workflows/ci.yml) | Push / PR to `master` | Vulnerability scan, build, test, pack, upload artifacts |
| [CodeQL](.github/workflows/codeql.yml) | Push / PR to `master`, weekly | Static analysis for C# |
| [Release](.github/workflows/release.yml) | Tag `v*.*.*` or manual dispatch | Build, test, pack, publish to nuget.org |

[Dependabot](.github/dependabot.yml) opens weekly pull requests for NuGet and GitHub Actions dependency updates.

### Continuous integration

The CI workflow runs on `ubuntu-latest` with Docker available for Testcontainers integration tests (PostgreSQL, MySQL, SqlServer). Packaged `.nupkg` files are uploaded as workflow artifacts and can be downloaded from the Actions run summary.

To build locally:

```bash
dotnet build src/LabelKit.sln -c Release
dotnet test src/LabelKit.sln -c Release --no-build
dotnet pack src/LabelKit.sln -c Release --no-build -o ./artifacts/packages
```

### Versioning

Package versions are calculated automatically by [MinVer](https://github.com/adamralph/minver) from Git tags:

- Tag `v1.2.3` → NuGet version `1.2.3`
- Commits after a tag → pre-release versions such as `1.2.4-preview.5`

Tag with a `v` prefix; published packages use plain semver without the prefix.

### Releasing to NuGet

**Tag push (primary release path):**

```bash
git tag v1.0.0
git push origin v1.0.0
```

**Manual release:** Actions → **Release** → **Run workflow** → select a branch or tag ref → toggle **Use GitHub release environment**.

#### Repository variable

For tag pushes, set **`USE_RELEASE_ENVIRONMENT`** under **Settings → Secrets and variables → Actions → Variables**:

| Value | Behavior |
|---|---|
| `true` or unset | Uses GitHub environment `release` (approval gate if configured) |
| `false` | Runs without a GitHub environment |

Manual workflow runs use the **Use GitHub release environment** checkbox instead of this variable.

#### One-time setup

1. On [nuget.org](https://www.nuget.org): create **Trusted Publishing** policies for this repository and `release.yml` workflow (configure one with environment `release` and one without if you need both modes).
2. In GitHub: create the **`release`** environment (optional required reviewers).
3. Add secret **`NUGET_USER`** (nuget.org profile name, not email):
   - **Environment secret** on `release` (when using the release environment)
   - **Repository secret** (when not using the release environment)

For full per-run flexibility, define `NUGET_USER` in both places with the same value.

## Examples

### EFCore PostgreSQL:

```csharp
public class Entity
{
  // Stored as JSONB
  public Dictionary<string, string> Labels { get; set; }
}
```

`dotnet add package aDisplayName.LabelKit.Parser`

LabelKit.Parser offers a default parser built with [Pidgin](https://github.com/benjamin-hodgson/Pidgin).

The parser is able to parse raw label-selectors adhering to the [kubernetes syntax](https://kubernetes.io/docs/concepts/overview/working-with-objects/labels/#label-selectors).

```csharp
using LabelKit;

var selector = LabelSelectorParser.Parse(
  "label1 = value, label2 = value, label3 in (value1, value2)");
```

`dotnet add package aDisplayName.LabelKit.EFCore.PostgreSQL`

```csharp
var expressionBuilder = NpgsqlLabelSelectorExpressionBuilders.Jsonb<Dictionary<string, string>>();

var entities = await dbContext.Set<Entity>()
  .MatchLabels(e => e.Labels, selector, expressionBuilder)
  .ToListAsync();
```

Executed SQL:
```postgresql
-- @__json_1='{"label1":"value","label2":"value"}' (DbType = Object)
-- @__Format_2='{"label3":"value1"}' (DbType = Object)
-- @__Format_3='{"label3":"value2"}' (DbType = Object)
SELECT t."Id", t."Labels"
FROM "TestEntity" AS t
WHERE t."Labels" @> @__json_1 AND (t."Labels" @> @__Format_2 OR t."Labels" @> @__Format_3)
```

### Label-Selectors

Label-selectors can be easily created and extended...

```csharp
var selector = LabelSelector.New()
  .Match("label1").Exact("value")
  .Match("label2").Not("value")
  .Match("label3").In("value1", "value2")
  .Match("label4").NotIn("value1", "value2")
  .Match("label5").Exists()
  .Match("label6").NotExists()
  .Match("label7").Like("^val.*")
  .Match("label8").NotLike("^excl.*");
```



They can be merged...

```csharp
var selector1 = LabelSelector.New()
  .Match("label1").Exact("value");

var selector2 = LabelSelector.New()
  .Match("label2").Exact("value");

// Contains a fully copy of all expressions
var merged = selector1.Merge(selector2);

// You can merge an arbitrary number of selectors

merged = LabelSelector.Merge(selector1, selector2, ...);

```

They can be rendered...

```csharp
// label1 = value, label2 = value
merged.ToString();
```

#### Regular expression matching (Like / NotLike)

LabelKit extends the kubernetes label-selector syntax with `like` and `notlike` operators for regular-expression matching on label **values** (not label names).

```csharp
var selector = LabelSelector.New()
  .Match("label1").Like("^val.*", "^value1$")   // value matches any pattern
  .Match("label2").NotLike("^excl.*");         // value matches none of the patterns
```

Patterns use [.NET regular expression](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions) syntax. Multiple patterns can be passed to a single expression; they are combined with OR semantics for `Like` and AND semantics for `NotLike`:

| Operator | Label absent | Label present |
|---|---|---|
| `Like` | no match | value matches **at least one** pattern |
| `NotLike` | match | value matches **none** of the patterns |

Offline matching applies a default regular-expression timeout of **200ms**. Customize `MatchTimeout` and `RegexOptions` via `MatchingOptions`, and pass it to `Matches(...)` or `LabelSelectorParser.Parse(...)`:

```csharp
var options = new MatchingOptions
{
  MatchTimeout = TimeSpan.FromMilliseconds(500),
  RegexOptions = RegexOptions.IgnoreCase
};

selector.Matches(labels, options);
// or
var parsed = LabelSelectorParser.Parse(raw, options);
```

Timed-out or invalid patterns are treated as a non-match: `Like` fails and `NotLike` passes.

When rendered as a string:

```csharp
// label1 like (^val.*, ^value1$), label2 notlike (^excl.*)
selector.ToString();
```

`Like` and `NotLike` work for offline matching (see [Matching Labels Offline](#matching-labels-offline)) and for EF Core queries across all supported providers (JSONB/JSON columns and string collections).

### Matching Labels Offline

You can also use label-selectors offline without any database interaction.

`dotnet add package aDisplayName.LabelKit`

```csharp
var selector = LabelSelector.New()
  .Match("label1").Exact("value")
  .Match("label2").Exact("value");

string[] labels = [ "label1:value", "label2:value" ];

// Default delimiter is ':'
// -> true
var doesMatch = selector.Matches(labels);
```

Regular-expression matching works the same way offline:

```csharp
var selector = LabelSelector.New()
  .Match("label1").Like("^val.*")
  .Match("label2").NotLike("^excl.*");

string[] labels = [ "label1:value1", "label2:other" ];

// -> true
var doesMatch = selector.Matches(labels);
```

The same can be done with dictionary-like labels:
```csharp
var selector = LabelSelector.New()
  .Match("label1").Exact("value")
  .Match("label2").Exact("value");

var labels = new Dictionary<string, string()
{
  ["label1"] = "value",
  ["label2"] = "value"
};

// -> true
var doesMatch = selector.Matches(labels);
```

> [!TIP]
> 
> Any component implementing **ILabelSelector** (meaning it can provide a collection of selector-expressions) can be used to match offline.

## Expressions (EFCore)

LabelKit supports infrastructure for building expression-trees that can be used to filter IQueryables.
Components implementing **ILabelSelectorExpressionBuilder** are responsible for creating **Expressions** from label-selectors.
Different expression-builders are needed for different scenarios.

Example: If your labels are stored as JSONB (PostgreSQL), the resulting SQL query has to be
vastly different from if they were stored as an array. Therefore, you need a different expression.

### Supported EFCore Providers

- [PostgreSQL](https://github.com/npgsql/efcore.pg) (>=5.0.5)
  - Supports labels stored as [JSONB](https://www.npgsql.org/efcore/mapping/json.html#traditional-poco-mapping-deprecated) ("name": "value")
  - Supports labels stored as [primitive-collection](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#primitive-collections) of strings ("name{separator}value").
- [MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) (>=3.2.0)
  - Supports labels stored as [JSON](https://dev.mysql.com/doc/refman/8.0/en/json.html) ("name": "value")
  - Supports labels stored as [primitive-collection](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#primitive-collections) of strings ("name{separator}value").
    - Disabled by default in the provider due to [this issue here](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/pull/1791).
- [SqlServer](https://github.com/dotnet/efcore) (>=8.0.0)
  - Supports labels stored as [primitive-collection](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#primitive-collections) of strings ("name{separator}value").
- [Sqlite](https://github.com/dotnet/efcore) (>=8.0.0)
  - Supports labels stored as [primitive-collection](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#primitive-collections) of strings ("name{separator}value").
- Any other EFCore provider supporting primitive-collections
  - Supports labels stored as [primitive-collection](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#primitive-collections) of strings ("name{separator}value").

### Provided Expression-Builders

- `NpgsqlJsonbLabelSelectorExpressionBuilder` (`NpgsqlLabelSelectorExpressionBuilders.Jsonb()`)
  - Builds expression specifically suited for PostgreSQL JSONB columns.
- `MySqlJsonLabelSelectorExpressionBuilder` (`MySqlLabelSelectorExpressionBuilders.Json()`)
  - Builds expression specifically suited for MySql JSON columns.
- `CollectionLabelSelectorExpressionBuilder` (`LabelSelectorExpressionBuilders.Collection()`)
  - Builds generic expression suitable for any collection of strings.
  - Supported by PostgreSQL, SqlServer, Sqlite (and MySql).
  - Available in package `aDisplayName.LabelKit.Expressions`.

> [!IMPORTANT]
> 
> `CollectionLabelSelectorExpressionBuilder` produces expressions that should be translatable to SQL by EFCore
> providers supporting [primitive-collections](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#primitive-collections).
> However, you should also be able to use those expressions offline (compiled).

### Filter IQueryables

`dotnet add package aDisplayName.LabelKit.Expressions`

```csharp
using LabelKit;

var expressionBuilder = LabelSelectorExpressionBuilders.Collection<string[]>();

// e => e.Labels is an expression representing the labels to match.
// You can also mark your entity as ILabelledEntity to avoid having to specify this every time.
var entities = await dbContext.Set<Entity>()
  .MatchLabels(e => e.Labels,
    selector => selector
        .Match("label1").Exact("value")
        .Match("label2").Exact("value")
    , expressionBuilder)
  .ToListAsync();
```
