# XmlSigner

Dotnet worker with self hosted web api. This worker is used for sign XML documents with Enveloping Signature.
Self hosted webapi provided 2 endpoints:
1. Provide all local certificates stored into LocalMachine certificate store
2. Sign a provided XML as base 64 encoded string with provided certificate
