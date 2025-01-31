# BigRedProf.Data.Tape

The **BigRedProf.Data.Tape** library allows data to be stored on tapes. Each tape consists of data (called content) and metadata (called label). The content is capped in length at one billion bits. Large data sets, like backups, can be spread accross multiple tapes using labels to help manage the tape relationships and ensure data integrity.

## Glossary

**tape** - a storage container consisting of a header, label, and content

**header** - a 1,000-bit (125 B) preamble to the tape containing things like a version and the offset to content

**label** - a *FlexModel*, up to 1,000,000-bits (125 KB) in pack length, containing arbitrary metadata about the tape expressed as traits

**content** - up to 1,000,000,000 bits (125 MB) of arbitrary data

---

**tape provider** - a specific implementation that allows abstract tapes to be stored across various data platforms (like a file on disk or an object in an S3 bucket); this library contains basic tape providers like MemoryTapeProvider and DiskTapeProvider, but others are free to add additional implementations to support more storage platforms in other libraries 

## Well-known traits
Tapes don't require any label traits. And they support arbitrary label traits. But there are also a handful of well-known traits that can be used to help manage tapes. These include
- WellKnownTrait.Guid - A globally unique identifier for this tape. Eg: 1AF4-13-1995
- WellKnownTrait.SeriesName - The common name for a series of tapes. Eg: "Tom's Photo Archive"
- WellKnownTrait.SeriesNumber - The number of this tape within a series. 25
- WellKnownTrait.Sha256Hash - The SHA-256 hash of the content of this file (excludes label and header).
- WellKnownTrait.RunningSha256Hash - The SHA-256 hash that consists of summing all previous hashes in this series including the hash of this tape itself.


## Fun Stuff
The header is a human-readable format, so you can run commands like
- cat *
- "for file in *; do [ -f "$file" ] && head -c 125 "$file" && echo ""; done"
to quickly see handy traits like the series name and series number of each file

## License

BigRedProf.Data is licensed under the MIT License. See LICENSE for more information.

## Contact

For questions, suggestions, or issues, please contact Professor at [BigRedProf@outlook.com](BigRedProf@outlook.com).
