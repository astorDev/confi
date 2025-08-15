Since first calls to `AddJsonHttp` happens before self-declaration it will fail (no appId available).

**Possible Solutions:**

- Send empty node definition with initial version (like `00000`) - will be overriden by first actual declaration, but avoids errors
- Add some sort of "after" argument in json http
    - accepting `Task` or what?
    - Issues:
        - Json configuration will not available before starting (and in the first few sec)
- Use sequential background service instead of json http
    - Issues: 
        - Binds stuff more tightly
        - Json configuration will not available before starting (and in the first few sec)