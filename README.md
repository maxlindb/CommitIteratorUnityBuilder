This is an app that builds a MAC build of your unity project for every commit backwards from the given commit, with a configurable interval, like every 10 commits. Builds from every run are put to a run-specific folder and contain git hash and part of commit message for easy tracking.

Note that the configuration is hard-coded to consts in the project, and the system doesn't do the initial clone and expects a project to exist in the given folder.

To use, change the "configuration" consts in code, ensure you actually have an unity project in the target folder and run.
