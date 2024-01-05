GoogleTaskExport [![Status Zero][status-zero]][andivionian-status-classifier]
================

This is a program to export the [Google Tasks][google-tasks] data from your Google account to a JSON file, or just print them in the terminal.

Usage
-----

### Client Secret
To use GoogleTaskExport, you'll have to register an application with Google and obtain a client secret file.

1. Create a Google Cloud Project.
2. On the **Enabled APIs & services** page, choose **Enable APIs and Services**, search for **Google Tasks API** and choose **Enable**.
3. On the **Google Tasks API** page, click **Create Credentials**.
4. On the **Which API are you using?** step, choose **User data**.
5. On the **OAuth Consent Screen** step, configure it whatever you want.
6. On the **Scopes** step, choose Google Tasks API (the non-readonly variant), `https://www.googleapis.com/auth/tasks`.
7. On the **OAuth Client ID** step, choose **Desktop app**. Specify the name as `GoogleTaskExport`.
8. Download the resulting JSON file.
9. Remember to add the users you want to use the application on the **OAuth consent screen** page, in the **Test users** list.

### Running the Application
Use the following shell command:

```console
$ dotnet run --project GoogleTaskExport -- [json|print] <username> <path to the JSON credential file>
```

This command will fetch all the stored Google Tasks (including the completed ones, but omitting the deleted ones) and output them in the specified format. 

Note that on the first run, it will open a browser window to ask you to grant the application access to your Google Tasks data.

If you want to flush the credential cache, delete the `<homePath>/Google.Apis.Auth` directory (or inspect the files and delete only the ones you want to flush; note that this is the default location for Google API libraries, so other programs may store their authentication tokens there as well), where `<homePath>` is `%APPDATA%` on Windows.

Documentation
-------------

- [Contributor Guide][docs.contributing]
- [License (MIT)][docs.license]
- [Code of Conduct (adopted from the Contributor Covenant)][docs.code-of-conduct]

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier#status-zero-
[docs.code-of-conduct]: CODE_OF_CONDUCT.md
[docs.contributing]: CONTRIBUTING.md
[docs.license]: LICENSE.md
[google-tasks]: https://support.google.com/tasks/answer/7675772?hl=en
[status-zero]: https://img.shields.io/badge/status-zero-lightgrey.svg
