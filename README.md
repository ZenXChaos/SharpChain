# SharpChain
Lite version of the Blockchain. C# Data integrity.

A light-weight framework that reads/writes to a blockchain file.

## BlockchainLite
This lite version of Blockchain reads and writes to a local blockchain. With SharpChain, it's easy to check for inconsistencies and tampering of data

### Blockchain Block Format

type     | size | description
:---------|:---:|:-------------
uint32 | 4 | Magic
uint8 | 1 | Block format
int | 4 | Timestamp (in seconds)
uint32 | 4 | Data length
data | Data.Length? | Arbitrary data
byte[] | 64 | Previous hash
byte[] | 64 | Block hash
uint32 | 4 | Owner length
byte[] | Owner.Length?| Owner Text
uint8 | 1 | Terminator / End of block

### Hash Calculation

The sha256 hash is created by concatenating the following:

`MasterKey+(Timestamp (in seconds(string)) + Previous Hash(string) + Data.Length(string) + Data(string) + Owner.Length(string) + Owner(string))`

Providing a format other than that will create inconsistencies with the data.

### Data inconsistencies

SharpChain checks for two types of data inconsistencies.

* Tampering with data
* Altering hashes.

Either will cause all future blocks to be inconsistent.

## Genesis Block

The initial Block Hash should always equal 64 '0's .

## Inconsistency check

After running your applciation (which will build a database), do some hex-editing then run the application again :).

### Indexing 

Indexing allows you to more quickly obtain a block in the Blockchain file. The index file is located in the same folder as the Blockchain appended with an ".idx" extension.

You can search a block by it's Hash.

`public static BlockchainIndex IndexOf(string Hash)`

`BlockchainIndex.IndexOf("5e01a7626e4dfb9f47eaf3ffe3c417170afae9c330a2b8f6b9658759a9a63943")` would return a `SharpChain Block`.

It's best practice with this utility to track hashes which you'll need to reference to avoid reading the index as much as possible. Especially for extremely large databases. 
I plan to implement an splitting feature soon. 👍 

### Wiki

### Credits
* [https://twitter.com/montraydavis](@MontrayDavis)
* [https://twitter.com/chaoticadev](@ChaoticaDev)
