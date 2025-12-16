
MediatR routes messages from the controller via type. Each message type has a corresponding handler that processes the message.

The Repository doesn't know about MediatR the handlers or the handler types. It only has access to the DTOs in Insfrasturucture. So, the handlers can't just pass their "handler type" to the repository methods. If the handler type has the same properties as the DTO, the handler type can be a simple derived class from the DTO and then the handler can just pass the DTO to the repository (see the PUT handler for an example). If the handler type has different (or a subset of) properties than the DTO, then the handler type should NOT derive from a DTO but be defined explicitly. The parameters needed by the repository can be passed as separate parameters (see POST handler for an example).

MediatR is used in the API to decouple the controllers from the business logic. Each controller action sends a request to MediatR, which then routes the request to the appropriate handler based on the request type. The handlers contain the business logic and interact with the repository to perform CRUD operations on the data.

In this example, the Keys table has an internal-only ID field that is not exposed via the API. The repository works with this internal ID field, but the ID field doesn't leave the repository. This is ideal and isn't always possible (e.g. when the ID is a foreign key to other tables).

Also, in the PUT repository method, there is too much logic that can't be unit tested.


