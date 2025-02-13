# Night of The Living Mimic

This mod adds quite a bit (Configurable) to the mimics featuring the following:

## Dead Bodies
Dead bodies can now come back as mimics and you can change:
- how often the check for this happens
- how rare it is for a body to turn into a mimic
- the mimics have a lower chance every time they come back
    - Eventually they can't come back to life at all
    - This is on a per mimic/body basis
    - this is Configurable with the "Chance decrease" option

## Normal Mimics
Normal Mimics that spawn can also come back to life randomly, and they follow the same spawning rules as the dead bodies. You can turn off normal mimics reviving with the "Masked turn back" mode

### Chaos mode
makes it so whenever you pick up a body it instantly turns into a mimic (mainly a testing thing for me)
The cursed steamIDS field in the config does this too to whoever you put in there (only as a smallish chance)


## Twitch Integration

I have added Twitch Integration thanks To Crit's [Twitch Chat API](https://thunderstore.io/c/lethal-company/p/Zehs/TwitchChatAPI/)

If you want to use this mod without twitch, you can disable or uninstall the twitch chat api without issues.

Currently the Twitch Integration does the following:
- Spawns a mimic on subscribes
- Spawns mimics on raids for each raider
- Spawns mimics based on how many bits given
  All of these are configurable and each part can be turned off if you don't want some of it.

Also in lobbies currently only the host has access to use twitch integration unless the host allows someone else to use it, either with the command `==ALLOW {gameUsername}` (without the brackets) or by putting their steamID into the config in the Twitch section.

You can also remove someone from that with `==DENY {gameUsername}` (without the brackets)

Also the broadcaster can put wsm# (replace # with an number ex. wsm5) in the chat to spawn that many mimics on a random player
if specified they can spawn a mimic on a player with `wsm# on {inGameName}` ex. `wsm5 on slayer6409` would spawn 5 mimics on the player named slayer6409. (There is a hard limit)

## DevTool Stuff
Currently there is a DevTool left in this mod (mainly for me and my friends to mess with eachother). By default only the host and a handful amount of people can use this.
I have made it so if the host puts someone's steam ID in the Allowed chat steam ids field they can also use this tool.
It basically allows for use of the popup chatbox.

To use it:
- Start the chat with ==
- Then put whatever you want in the top portion
- then put & followed by whatever you want in the bottom portion
- like so: ==Glitch & is bald
  and you will end up with something like this:
  ![](https://i.imgur.com/egjXE7b.png)
- you can also target someone and send it only to them
- like so: ==Glitch&is bald } aglitchednpc
- that would send the Glitch is bald message directly to whoever has the closest name to "aglitchednpc"
