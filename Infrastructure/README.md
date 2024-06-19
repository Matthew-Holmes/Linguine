# Infrastructure

The Linguine app uses a layered architecture, with Infrastructure the lowest layer.

The core class is `LinguineDataHandler` which implements ORM using Entity Framework. `Dataclasses/` contains the class definitions that map to tables. The entities store the data required for importing, processing and retrieving text.

Some utility classes are also implemented, such as in `DataAggregators/` which includes a `Dictionary` class, to look up `DictionaryDefinition` objects, the data class used in the ORM.

The task of breaking down text involves the production of `Statement` objects, normally a sentence decomposed into words and their roots. These form a sequence sharing contextual information, due to the structure and data duplicaton present in a `Statement` object, they are stored in a memory efficient format, the implementation of which is visible only in the Infrastructure module.

Infrastructure also contains classes to parse .csv files into the database entries that make up the `Dictionary` table

