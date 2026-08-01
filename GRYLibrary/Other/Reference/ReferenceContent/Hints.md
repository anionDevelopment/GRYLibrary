# Hints

## Signing

The GRYLibrary-NuGet-packages are always signed. You can check the public key token by using [sn](https://docs.microsoft.com/en/dotnet/framework/tools/sn-exe-strong-name-tool): `sn -T GRYLibrary.dll`

The public key token of all official GRYLibrary-releases is `fa37b6e9de549c68`. For security-reasons you should only use GRYLibrary.dll-files which you have compiled by yourself from the source code in this repository or which have this public key token.

## Requirements

The following tools from the [tools-list](https://github.com/anionDev/ScriptCollection/blob/main/ScriptCollection/Other/Reference/ReferenceContent/Articles/RequirementsForCommonProjectStructure.md) are required to build this code-unit:

- `docfx`
- `dotnet`
- `dotnet-coverage`
- `git`
- `gitversion`
- `python`
- `pygmentize`
- `reportgenerator`
- `scriptcollection`

## IDE

The recommended IDE for this codeunit is [Visual Studio](https://visualstudio.com/).

## Running the tests on developer-machines

On Windows the command `echo` is a built-in command of `cmd` and not a standalone executable. Therefore it can not be resolved and executed like a regular program. For this reason the testcases which need to execute an echo-like program (for example `ExternalProgramExecutorTest.TestVerboseExecutionProducesExpectedStdOutLogSequence`) expect a program named `echo2` to be available in the `PATH` on Windows-developer-machines. `echo2` is expected to write its first argument to the standard-output (just like the unix-`echo` does).

On Linux and macOS the regular `echo`-executable is used instead, so no additional program is required there.

## Known issues and open topics

The following list contains findings which are known but which are not fixed yet because they require a larger refactoring, a decision about the desired behavior or an implementation of a missing feature. (Findings which are located in the `APIServer`-namespace are not part of this list.)

### Not implemented operations

These operations exist but throw a `NotImplementedException`:

- `Misc.Utilities.GetBytesArraysFromConcatBytesArraysWithLengthInformation` (the inverse operation `ConcatBytesArraysWithLengthInformation` is implemented)
- `Misc.Utilities.BinaryStringToBigInteger` and `Misc.Utilities.BigIntegerToBinaryString`
- `Graph.Graph.GetShortestPath` (planned to be implemented using Dijkstra's algorithm)
- `Graph.Graph.IsSubgraph`; because of that also `Graph.Graph.IsSubgraphOf` and `Graph.Graph.IsIsomorphic` do not work
- `Misc.TableGenerator` for the output-type `HTMLTable`
- `Streams.StreamMixer.StreamPipe.Start` and `Streams.StreamMixer.StreamPipe.Stop`; therefore the whole `StreamMixer` is not usable
- `Misc.EarleyParser` is not finished (it is `internal` for that reason). Additionally the branching in `EarleyParser.Parse` looks inverted: the `Scanner` and the `Predictor` are executed if the state is finished and the `Completer` is executed if it is not finished, while it is supposed to be the other way round.

### Behavior which does not match the name or the documentation

- `Misc.Similarity.CalculateJaccardIndex` does not calculate the mathematical Jaccard-index (which is defined on sets). It calculates the ratio of the amount of characters of the first string which also occur in the second string to the sum of the lengths of both strings. Because of that `CalculateJaccardSimilarity` multiplies the result by 2 to reach the value 1 for equal strings. The helper-function `CalculateSimilarityHelperGetUnion` also does not calculate a union but a concatenation. The current behavior is asserted by the existing testcases in `SimilarityTest`, so changing it requires a decision about which semantics are desired.
- `Misc.Utilities.RunAllConcurrentAndReturnFirstResult` is documented to return the result of the first execution which does not throw an exception, but exceptions of the executed functions are not caught. They abort `Parallel.ForEach` and reach the caller as `AggregateException` instead.
- `Graph.Graph.Equals` is documented to ignore the names of the vertices and the edges, but it compares the vertices (which are equal if and only if their names are equal) and it builds the adjacency-matrices based on the alphabetical order of the vertex-names. Additionally it compares the vertices using `SequenceEqual` on hash-sets, so the result depends on the enumeration-order of the hash-sets.
- `Misc.Utilities.GetRandomHexCharacter` returns as many hex-characters as stated by its parameter `digits` and not one single character as its (singular) name suggests.
- `Misc.Utilities.IsSelfSIgned` contains a typo in its name. Fixing it changes the public API.
- `Misc.ByteArray.CreateByHexString`, `CreateByInteger` and `CreateByString` are factory-operations but they are implemented as instance-operations, so an already existing `ByteArray`-instance is required to create a new one. Making them `static` changes the public API.

### Thread-safety

- `Misc.MultiSemaphore` is documented as threadsafe, but `Decrement` checks the value and modifies it afterwards without holding a lock over both operations. Two concurrent `Decrement`-calls can therefore both pass the check.

### Robustness and edge-cases

- `Misc.Similarity.CalculateCosineSimilarity` divides by zero if both strings are empty. The resulting `NaN` is silently converted to 0 by `Misc.PercentValue`. A defined result for this case should be specified.
- `AOA.EqualsHelper.CustomComparer.DictionaryComparer.EqualsTyped` searches the key using the configured custom comparer but reads the value using the indexer of the dictionary (which uses the default comparer of the dictionary). If both comparers do not agree, a `KeyNotFoundException` is thrown.
- `Misc.Utilities.Cast` contains a mechanism for custom conversions (`DefaultConversions`), but its implementation is only a stub: the body of the loop over the custom conversions is commented out. For that reason `Cast2` is still `internal`.
- `Misc.Utilities.EscapeForCSV` only quotes values which contain a double-quote. Values which contain the separator or a line-break are written unquoted and therefore break the structure of the resulting CSV-file.
- `Misc.Utilities.MoveContentOfFoldersAcrossVolumes` does not create the directories of the source-folder in the target-folder (see the TODO in its directory-action). Empty directories are therefore not moved.
- `Misc.Utilities.FileEndsWithEmptyLine` throws an exception if the given file is empty.
- `Misc.TableGenerator.Generate` does not check that the amount of the given headlines is equal to the amount of columns of the given array.
- `Playlists.ConcretePlaylistHandler.PlaylistLoader.LoadItems` adds the not-normalized item to the list of the not existing items in its last branch while all other branches add the normalized item.
- `Misc.GRYDateTime`, `Misc.GRYDate` and `Misc.GRYTime` override `Equals` but use the default implementation of `GetHashCode` of `System.ValueType`. That is correct but it results in a bad distribution of the hash-codes because only the first field is taken into account.
