# Learning Extraction

In Eric Evans' language, LearningExtraction is the **core domain** of the app, responsible for breaking down media (for now only textual) into items to learn - here vocabulary items.

Doing this requires forming prompts to LLM agents for a sequence of tasks, such as decomposing text, then resolving definitions. Then parsing the responses back to the data structures required by the larger model.

The core classes here are `TextDecomposition` and `Statement`. `TextDecomposition` stores the strings representing the units composing a sentence. These can be words, or composite words. The implementation uses a tree data structure to support composite words, such as "garden centre" --> "garden" "centre", where definitions can be found for both the composite word, and its constituents.

For now there is only support for a single level tree, since handling composite words is a secondary concern - however I wanted to make the data structure flexible enough to handle it in the fullness of time.

Language is borrowed from set theory to maintain invariants, in the case of the first pass decomposition, that must be "injective" - so that the words found are exactly present in the text and can be highlighted for the user. The second pass is to root those words, forming a "bijective" map between the injective words, and their rooted counterparts. These invariants are checked as part of the processing, and fall backs are implemented when they are breached.

## `Statement`
I opted to not process the whole text at once, but as chunks called "Statements", which are processed in batches, a `Statement` object usually is a sentence, I didn't make the entire text one whole `TextDecomposition` object, partially using domain driven reasoning, since that implies the relationship between words and sentences is the same as the relationship between sentences and text. This seems correct on a surface level, however a sentence is normally only understood when all words have been considered, whereas a text can be paused at the end of any sentence. 

Eric Evans emphasises identifying a mismatch between technical models and domain models, in this case I chose to introduce the `Statement` class to resolve this mismatch, which then led to a natural way to consider contextual information.

Forming these statements is performed using a `StatementBuilder` class, inspired by the C# `StringBuilder` class.

