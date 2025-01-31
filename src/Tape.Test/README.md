# BigRedProf.Data.Tape

The **BigRedProf.Data.Tape** library provides a structured way to store and manage large datasets across multiple tapes. Each **tape** consists of data (called **content**) and metadata (called a  **label**). The content of each tape is capped at one billion bits. Large data sets, like backups, can be spread accross multiple tapes using labels to help manage the tape relationships and ensure data integrity.

## Glossary

**tape** - a storage container consisting of a header, label, and content

**header** - a 1,000-bit (125 B) preamble to the tape containing things like a version and the offset to content

**label** - a *FlexModel*, up to 1,000,000-bits (125 KB) in pack length, containing arbitrary metadata about the tape expressed as traits

**content** - up to 1,000,000,000 bits (125 MB) of arbitrary data

---

**tape provider** - a specific implementation that allows abstract tapes to be stored across various data platforms (like a file on disk or an object in an S3 bucket); this library contains basic tape providers like MemoryTapeProvider and DiskTapeProvider, but others are free to add additional implementations to support more storage platforms in other libraries 

---

## **\ud83d\udccc Tape Structure**
Each **tape** consists of **three parts**:

| **Section**  | **Size**          | **Description** |
|-------------|-----------------|----------------|
| **Header**  | **1,000 bits (125 B)** | Stores version info and label length. |
| **Label**   | **Up to 1,000,000 bits (125 KB)** | Stores metadata as a `FlexModel`, using well-known and custom traits. |
| **Content** | **Up to 1,000,000,000 bits (125 MB)** | The actual data stored in the tape. |

---

## ** Header **
The header begins with a **100-byte ASCII string** that can be read easily via `cat` or `head -c 100` from disk tapes.

The remaining 25 bytes contain things like the length of the packed label.



## **\ud83d\udccc Well-Known Traits**
Tapes support **arbitrary metadata traits**, but the following **well-known traits** help with **managing multi-tape archives**:

| **Trait**               | **Description** |
|-------------------------|----------------|
| **`WellKnownTrait.TapeId`** | A **globally unique ID** for this tape. (e.g., `"1AF4-13-1995"`) |
| **`WellKnownTrait.SeriesName`** | Name of the **series** this tape belongs to. (e.g., `"Tom’s Photo Archive"`) |
| **`WellKnownTrait.SeriesNumber`** | The **number** of this tape within the series. (e.g., `25`) |
| **`WellKnownTrait.Sha256Hash`** | The SHA-256 hash of **only the content** (excluding header & label). |
| **`WellKnownTrait.RunningSha256Hash`** | The cumulative **SHA-256 hash of all previous tapes** in the series, plus this one. |

---

## **\ud83d\udccc Fun Stuff**
Since the **header is human-readable**, you can inspect tapes with standard UNIX commands:

#### **\ud83d\udcdd See the Header of All Tapes in a Directory**
```bash
head -c 125 *
```
#### **\ud83d\udcdd See the First 125 Characters of Each File**
```bash
for file in *; do [ -f "$file" ] && head -c 125 "$file" && echo ""; done
```
#### **\ud83d\udcdd See Series Names and Numbers**
```bash
for file in *; do [ -f "$file" ] && head -c 125 "$file" | grep "Series"; done
```

---

## **\ud83d\udccc License**
BigRedProf.Data is licensed under the **MIT License**. See `LICENSE` for details.

## **\ud83d\udccc Contact**
For questions, suggestions, or issues, contact **Professor** at [BigRedProf@outlook.com](mailto:BigRedProf@outlook.com).
