# Learning 

A lot of the work on this has been inspired by spaced repition systems (SRSs) such as Anki or Memrise. These are very useful, and help optimise what is presented to the user.

However, there are some drawbacks, some of which occur only after long periods of use. One way to combat a lot of these is to have access to the underlying distribution of words that the user is learning.
This saves the user time in curating their vocabulary lists, while avoiding a "one size fits all" approach, (Duolingo). Since the user provides only text that they are interested in, the system picks out the most pertinant words for them to learn.

For example a user interested in reading books in their target language, will probably be interested in different words to a user learning for professional reasons.

Instead of a system that presents a fixed amount of study per day, I opted to have an endless cycle of flashcards, and the user can choose to study for as long/as little as they like. 
Later I may add policy systems to direct how time should be allocated (however there are currently no other activities)

## Key idea

Below is a table of the issues with existing SRS systems, and how Linguine mitigates them:


| Problem                                  | Description                                                                                                        | Solution                                                                                                                                                  |
| ---------------------------------------- | ------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Leeches                                  | pathological words that the user frequently has to relearn                                                         | track exposure to learnings via media, and only reintroduce them after sufficient re-exposure                                                             |
| Wasted time on easy learning             | often basic vocabulary is drilled unnecessarily                                                                    | once a learning's exposure rate is high enough - we no longer need to be actively testing for it, and it can be removed                                   |
| Overwhelming build up of learnings       | over time, the volume of tracked learnings means new vocabulary is watered down by revision of existing            | previous two, plus contextualised vocabulary introduction - target specific use cases/media only while the user is interested in that: use it or lose it! |
| Poor distribution of learning difficulty | user specified lists of learnings often include stuff that is way too easy/too hard                                | by forming an internal model of the users vocabulary, the system can introduce only the most beneficial new learnings                                     |
| False comprehension, missed nuance       | learning spelling to meaning mappings means that other uses of a word might be missed. I.e. failed disambiguation  | the dictionary definition list backstop, and learning extraction algorithm                                                                                |
| Hangovers                                | specific learnings from a previous goal, that the user is no longer interested in                                  | the "use it or lose it" approach combats this                                                                                                             |

There are some more pieces of ubiquitous language introduced here:
* **exposure** - this could be from the testing system, or inferred from the user stating they read/watched/listened to a piece of  media
* **natural exposure rate** - the rate (over time) at which a user is exposed to a learning naturally, as they consume more of the target language/the complexity of their consumption increased; this rate will alter
* **learning model** - a function that aims to predict if a user knows a learning or not

The key observation that enables an improved learning system, is that learning occurs holistically outside the application - if we can model that, then it supports much more efficient drilling/testing.

## The learning model

This is yet to be fully implemented, so far the only component is the `VocabularyModel` which models the probability that a user knows a given word's meaning.
Even with just this simple model, we can already produce quite powerful learning algorithms, that optimise for the probability a user knows a given word in the text provided.

The next step will be to consider time effects of exposures both via the flashcard system, and from passive exposures to vocabulary.

Pronunciations would also be helpful.
