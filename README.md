# Transifex.Backend
A tool to perform advanced queries on transifex.

## Pre requisites
If you have mongo and redis, you are all set. If not, you can use my docker image. You can read the [instructions here](https://github.com/g3rv4-docker/redis-and-mongo) to have both of them run on your localhost on the default ports.

## Running this
You should define the following environment variables (or, if you are using VS Code, update them at `.vscode/launch.json`).

```
"env": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "REDIS_CONNECTION": "localhost",
    "REDIS_DATABASE": "1",
    "TRANSIFEX_SESSION": "<your session cookie>",
    "MONGO_SERVER": "mongodb://localhost:27017",
    "MONGO_DATABASE": "transifex"
}
```

If you set `ASPNETCORE_ENVIRONMENT` to `Development`, then it's going to cache all the queries to Transifex for ever. Useful when debugging the update route.

Once those things are set up, `dotnet run` should just work... same thing with debugging from VS Code.

## Where is the backend?
Nowhere... yet

## Contributing
Contributions in all forms and shape are more than welcome, as long as you comply with our [code of conduct](https://github.com/g3rv4/Transifex.Backend/blob/master/CODE_OF_CONDUCT.md).
