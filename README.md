# DLL-dumper
Basic DLL dumper with modification checker from a process made in C#.

# How do I use it?
Run it as administrator, type in your process name (without the .exe!!" and let it do it's thing.

# Usage?
It's mainly for cheat cracking, like if a loader is really insecure you'll be able to find the dll directly in the loader or in the game itself post-injection.

# Important!
It will do an initial scan of the process, creating a folder with everything it found in that process.

If any file is different from the original injection, it will create a new folder and dump them inside of it. (aka injected a dll or something)
