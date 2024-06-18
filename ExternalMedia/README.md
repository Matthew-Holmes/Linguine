# External Media

This module acts as an anti-corruption layer, specifically normalising text encodings.

The class TextualMediaImporter attempts to identify which encoding a file to import uses, from:
* UTF8
* ASCII
* Unicode
* BigEndianUnicode
* UTF32

Then verifies with the user, if the user isn't happy it will generate previews of the file read using each encoding, so the correct one can be identified.

It is important to be extra rigorous here, since in a language learning app, text sources with a variety of encodings are likely to be imported.
