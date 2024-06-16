# BigRedProf.Data

The **BigRedProf.Data** library is a simple, flexible .NET library for defining and serializing models. 

## Glossary

**bit** - a discrete binary value; uses the symbols **0** and **1**

**code** - a sequence of one or more bits used to represent something (think cryptographic messages, NOT computer instructions)

---

**model** - a software developer's representation of a domain object

---

**packing** - the act of turning a **model** into a **code** (think serialization)

**unpacking** - the act of turning a **code** into a **model** (think deserialization)

**pack rat** - the actor responsible **packing** and **unpacking** a specific **model**

**pied piper** - the actor who organizes all the **pack rats** (consider creating a singleton pied piper in your startup code if you're using dependency injection)

---

**flex model** - a flexible **model** composed of one or more models called traits; the advantage of the flex model is that clients don't need to understand its full schema to work with it--they can use the traits they understand and ignore the rest

**trait** - a model and its purpose (eg: "My name is Memorial Stadium." or "My location is Lincoln, Nebraska.")

## License

BigRedProf.Data is licensed under the MIT License. See LICENSE for more information.

## Contact

For questions, suggestions, or issues, please contact Professor at [BigRedProf@outlook.com](BigRedProf@outlook.com).
