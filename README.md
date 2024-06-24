# Linguine
Language learning toolkit, includes LLM assisted learning.

This project is also an opportunity to explore ideas from Domain Driven Design. The practises proposed by Eric Evans in his book on the subject are attempted in this codebase. For more details [see here](#domain-driven-design)

Unit testing is also implemented, using the MSTest framework.

## Overview

Linguine is a Windows WPF app, it will include: tools to analyse user imported media for vocabularly and grammar to learn; spectographic pronunciation training; and a modified spaced repetition service to review learnings.

For implementation details see readme's in each subfolder (excluding tests).


## Current Progress

The app can import and store pieces of text, and the user can return to previous sessions. Once imported, text can be iteratively processed using the LLMs, which identifies words and resolves definitions - when homonyms are present.

The text that has been processed is highlighted and the user can interact with it, selecting words to see their root form and definition when it has been found.


<img src="https://github.com/Matthew-Holmes/Linguine/blob/master/Linguine/SampleImages/LinguineDemo001.png?raw=true" width="400"> <img src="https://github.com/Matthew-Holmes/Linguine/blob/master/Linguine/SampleImages/LinguineDemo002.png?raw=true" width="400">
<p align="center">
<img src="https://github.com/Matthew-Holmes/Linguine/blob/master/Linguine/SampleImages/LinguineDemo003.png?raw=true" width="400">
</p>

## Domain Driven Design

Domain driven design pulls focus away from the technical details of implementation frameworks. Instead looking to the domain, and the language used to describe it, to drive the structure of the software. In this project the domain is language learning. I read the book Domain-Driven Design by Eric Evans, which provides a sweeping description of what he views as best practises, from the design of classes and their interactions, up to the large scale structure and management of large projects.

A key idea of Domain Driven Design is the formation of a **Ubiquitous Language** that is used when describing the domain, and should be also used when naming classes, modules and services. the most extreme implementation of Domain Driven Design hinges on the idea that: "Changing code changes the model" - Eric Evans. Here the model is the model that developers and subject matter experts hold in their head of the domain.

### Definitions

Evans introduces lots of vocabulary to describe Domain Driven Design, some include:

**Ubiquitous Language** - used to describe the domain and problems by both experts and developers

**Bounded Context** - an area that has its own ubiquitous language

**Core Domain** - the boiled down, business specific domain model that should be focussed on

**Anti-Corruption Layer** - Designed to form a hard boundary between two models. It is noted how information is not just the formatted data, but the context and meaning surrounding the data. An anticorruption layer's role is to ensure there is no bleed in the meaning of data between two models . May be formed of facades, adaptors, translators and services.


### Application

At the end of the book, Evans lays out a process to begin implementing the ideas:

1. draw a context map - trying to resolve ambiguity
2. hone the ubiquitous language
3. identify core domain; attempt a domain vision statement

I attempted these:

#### Context Map

| Bounded Context        | Model                         | Notes                                        |
| ---------------------- | ----------------------------- | -------------------------------------------- |
| Linguistics            | spectrographic audio analysis | focus on audio signals and sound replication |
| External Media         | media normalisation           | anti-corruption layer                        |
| Learnings: Extraction  | services to extract learnings | heavy use of LLM APIs                        |
| Learnings: Store       | repository of things to learn | just "facts" - no tracking of recall         |
| Learnings: User Recall | recall tracking for a user    | spaced repetition, similar to ANKI           |
| Chat                   | chatbot access and config     | to practice using the language               |
| Analytics              | user data tracking            | to help guide policy                         |
| Policy                 | user learning direction       | use analytics                                |

#### Ubiquitous Language

For example the ubiquitous language in Learnings: Store

| Term             | Definition                                                            | Notes                                                                    |
| ---------------- | --------------------------------------------------------------------- | ------------------------------------------------------------------------ |
| Learning         | something that the user would consider a unit of knowledge            |                                                                          |
| Root Learning    | a learning which has some notion of purity                            | e.g. uninflected verb                                                    |
| Variant Learning | a learning that is not a root                                         | e.g. inflected verb                                                      |
| Implicit Variant | a variant that the user could reproduce given their current knowledge | e.g. plural, or learnt conjugation rule; the product of another learning |
| Explicit Variant | something the user would expect to learn distinctly                   | edge cases, different spellings                                          |
| Representation   | a learning's presentation to the user                                 | e.g. text/audio of a vocab; or grammar rule as a pattern                 |
| Knowledge Store  | repository of learnings                                               | without user scoring                                                     |

#### Core Domain

In this case the core domain is Learnings: Extraction. Breaking down text into vocabulary and grammar is not a simple task, and most other contexts require this process.

Tracking recall and storing learnings are both well understood domains, with "off the shelf" implementations that can be emulated. With this in mind the first step of the project was producing an app that could load and decompose pieces of text into "learnings", to do this LLMs were used. For implementation details see the LearningExtraction directory.

