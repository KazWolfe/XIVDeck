# Contributing to XIVDeck

First off, thanks for taking the time to contribute to XIVDeck! Your support, issues, and pull
requests really mean the world to me. It amazes me that people like this plugin enough to want
to help.

This document aims to set up a few *very* high level guidelines for what I would like to see
in any contribution to this repository. This document does not offer prescriptive rules, nor does
it exist to tell you what to do. This project is made open source so that anyone can see and help
with it, and not to allow me to experience what it's like to be a BDFL.

Really, I just ask that you keep a few major points in mind with this repository:

1. **Please respect the original intent of the project.**  
XIVDeck was created to balance on a very fine line of being a helpful tool. I do not want this 
project to become a "cheat" application, nor do I want to encourage users to use it in such a
way that would give users a material advantage over non-XIVDeck users. Things added to this
project should, broadly, make "utility hotbars" unnecessary to the average player.
2. **Keep the user experience pleasant and easy.**  
Everyone has a different idea of what entails a pleasant user experience. Mine is based on the
idea that this plugin just works. Icons show up automatically, users don't need to do any 
notable maintenance, and everything stays synchronized to the game. Where possible, please
preserve this intent with your suggestions and pull requests. Things that are highly technical,
require maintenance or upkeep, or otherwise can be unpleasant/burdenful to use should be avoided.
3. **Follow existing style guides and patterns.**  
I'll be blunt. I know my C# code looks like Java. I know I can optimize some things to LINQ. I
know there's a lot of things that can be cleaned up to bring this project "in line" with what one
would normally expect from a C# application or a Dalamud plugin. I'm open to people doing that,
but also please respect my codestyle and commit messages. Put the braces on the same line, try
to mirror my commit message style (or use Conventional Commits), and make sure things are 
appropriately documented. This'll help me maintain this project, and help keep others from
getting confused when styles change.

I also have a few more specific requests:

* Please do not alter version numbers anywhere. Let me take care of that when a release is due.
* Do not add new signatures to the code - please open a new issue and let me review/test things.
* Preserve the integrity of the API. If a breaking API change is necessary, plesae contact me.

Lastly, I want to make clear that *anything done in this repository will have to be something I
am able to support*. I may reject certain commits on these grounds - e.g. a pull request to add
macOS support will likely not be accepted purely because I don't have a Mac I can use to test
the code. Commits that add things like that are totally valid, but I feel that I have an
obligation to support this plugin in its entirety, and I want to be able to do that.

With all that out of the way... feel free to open any PR or issue you feel would help XIVDeck!