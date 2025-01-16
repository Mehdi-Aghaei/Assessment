# Assessment
A simple API to gather letter statistics from the Lodash repository.

## Build and Run
Open a browser or an API client of your choice and make a GET request to:  
**`https://localhost:7205/Statistics`**

### Important Note
- The first request may take up to 30 seconds to process.
- **You have to use your own Github API key you can get it by going to  github setting and DEVELOPER Setting then generate token**
- Replace your token in Processing Service line 18.

### Alternative you can use dotnet Secret in projec directory execute following
```bash
  dotnet user-secrets init
  dotnet user-secrets set "GithubToken" "YourGithubToken"
```
