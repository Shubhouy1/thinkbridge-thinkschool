# Why a Rich Quote Model?

The original Quote entity was anemic: it only contained properties and allowed any part of the application to modify them. Validation was also handled in the endpoint, which meant the domain object itself could exist in an invalid state.

The rich model moves these rules into the Quote domain. `Quote.Create(author, text)` is now the controlled way to create a quote and validates the author's and text's length. The entity also protects its state by making its properties private to set. Once created, the quote's text cannot be changed. Deletion is represented as a soft delete instead of physically removing the quote.

This makes the domain responsible for protecting its own invariants rather than relying on every caller to remember the rules.

For example, with the anemic model, another endpoint or service could create a Quote with an empty author or 2,000-character text simply by assigning the properties. The existing API validation would not protect that code path, so invalid data could reach the database. With the rich model, `Quote.Create()` rejects the invalid input at the domain boundary.

The rich model therefore reduces duplicated validation and makes invalid Quote states harder to create.