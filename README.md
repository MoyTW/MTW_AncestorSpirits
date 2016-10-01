# Summary

You Ancestors are watching you, and they are hard to impress. To ignore them would be foolish, though - they're the foundation of your tribal magic, and if they're displeased are more than capable of calling down storms, plagues, and divine punishments. If, on the other hand, you manage to honor and please your ancestors, they can gift you with Magic, which is the basis for all your Rituals - Rituals which can call rain or lightning, or heal sick or injured tribe members.

Build a shrine room:

![It's a pretty dull room, this one is.](https://github.com/MoyTW/MTW_AncestorSpirits/blob/master/About/Images/ShrineRoom.jpg)

Once you've established a Shrine, Ancestors will come visit:

![I'll work on the naked thought. Also, they should be ghostly (see planned features).](https://github.com/MoyTW/MTW_AncestorSpirits/blob/master/About/Images/AncestorCard.jpg)

Ancestors are immune to basically everything, flames included! Samantha and Nanor are just fiiiine here (and if you find something that can kill them, tell me so I can fix that):

![Can tank a Doomsday rocket to the face and take no injures!](https://github.com/MoyTW/MTW_AncestorSpirits/blob/master/About/Images/AncestorFlames.jpg)

If the Ancestors are pleased, they'll give you Magic, which you can use for Rituals:

![Some balance work needed.](https://github.com/MoyTW/MTW_AncestorSpirits/blob/master/About/Images/Petitions.jpg)

The actual petitioning is pretty dull-looking at the moment though:

![Maybe I should add a Petitioning skill? Maybe base it off Social? Probably just base it off Social.](https://github.com/MoyTW/MTW_AncestorSpirits/blob/master/About/Images/Petitioning.jpg)

I'll make a more detailed writeup Sometime Later, when I iron out a lot of these issues and make it more friendly and full-featured and Officially Release It, but you can probably derive how most of it works from the features list.

# Disclaimer

This mod is current very much a work-in-progress! It can be applied to in-progress games, but don't use it on any saves you're not prepared to use.

# Incompatibilities
+ It is incompatible with the Hospitality mod. I intend to patch it to be compatible (see planned features section).

# Planned Features

## DONE Useless Ancestors
+ DONE Ancestors will re-spawn if Shrine is destroyed
+ DONE Ancestor pool is fixed
+ DONE Ancestors have their own faction
+ DONE Ancestors hang around the Shrine(s)

## DONE Picky Ancestors
+ DONE Ancestors get very angry if you have two Shrines
+ DONE Ancestors are very displeased if you have no Shrines
+ DONE Approval score can increase or decrease
+ DONE Semi-random events based on Approval level
  + Flash Storm if Ancestors are angry
  + Drop Pods (change this later) if Ancestors are pleased
+ DONE Magic storage, based on Approval
  + Magic is seasonal (KODP is the reference point here)
  + Subtract from if the Ancestors are displeased
  + Add to Magic if the Ancestors are pleased

## DONE Magical Ancestors
+ DONE Basic weather rituals (use Magic)
  + Rain
  + Wind
+ DONE Advanced weather rituals (use Magic)
  + Warmth (heat wave)
  + Cold (cold snap)
  + End Strange Weather (cancel map conditions, clear weather)

## DONE Joyful Ancestors
+ DONE Ancestors have a Joy need
+ DONE Ancestors will use Joy objects
+ DONE Ancestors will not get stuck in a Joy-object using loop!

## DONE Homebound Ancestors
+ DONE Shrine building creates a "Shrine Room"
+ DONE Ancestors get a happy/sad thought for the Shrine Room
+ DONE Ancestors will get a sad thought if there is no Shrine Room
+ DONE Ancestors will periodically return to their Anchor

## DONE Invincible Ancestors
+ DONE Spirits are invulnerable to everything
+ DONE Ancestors cannot be arrested
+ DONE Ancestors will persist between despawns and saves
+ DONE Ancestors cannot be simply walled-in

# Bonus Planned Features
## DONE Variable-magic rituals
+ DONE Rituals now have a failure chance (Ancestors are indifferent)
+ DONE Rituals can be overbid/underbid to influence failure chances
+ DONE Control magic bid by lighting attached altar buildings

## DONE More Petitions 1
+ DONE Crop Growth: Your crops grow really fast
+ DONE Instant Healing: Your colonists are instantly healed
+ DONE Ancestral Fury: Lightning strikes your foes

## Hospitality-style visits
+ Ancestors trickle in/out instead of group spawning
+ Ancestors spawn for "visits" instead of constantly
+ Visits are semi-random but normalized to give same approval+/-
+ Approval changes submitted when Ancestors return to the spirit world
+ Ancestors vanish (huge approval hit) if shrine destroyed while visiting
+ Ancestors will occasionally walk outside of colony
+ Ancestors will get super mad if they can't return to their anchor

## Spritual Ancestors
+ Ancestors are colored differently or transparent or something distinct
+ Ancestors don't have nudist/clothing-based thoughts

## Beginner Affordances
+ Don't start timer on Shrines until after a certain time period has passed
+ Possibly add in a tutorial message?

## Blessings/Punishments
+ Add blessings/punishments instead of cargo pods/flashstorm
+ Create Defs for Blessings/Punishments
+ Punishments
  + Blight
  + Sickness (various)
  + Lightning strike

## Hospitality Compatibility

# This Stuff Would Be Awesome, But Are Not In Scope Right Now
+ Ancestor Actions
  - Ancestors will hunt down and compliment/insult colonists
  - Ensure Ancestors never do romantic actions?
  - Ancestors will occasionally wander outside the colony
+ Offerings
  - Altar at which you can offer things to your Ancestors to boost Approval
  - Requests from Ancestors that you have to fulfill, or lose Approval
  - Specific Ancestors having likes/dislikes in terms of offerings
+ Ancestor Events
  - Ancestral visit: many Ancestors manifest for a period of time
  - Quiet dead: Ancestors leave your colony for a period of time
+ GODLARP

# Your Code Sucks
+ Stop checking against the literal string "Spirit" to see if you can Do X
+ DONE Use Reflection to set the private weather ticker insteaed of...what you do