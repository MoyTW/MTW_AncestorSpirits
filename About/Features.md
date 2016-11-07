# Changelog

## v0.1a

+ Initial release

## v0.1b

+ Update README & Preview.png

## v0.1c

+ Update README with compatibility, install info
+ Fix NPE for visits with no anchors
+ Add 7-day grace period for No-Shrine visit penalties
+ Remove extra Cloth & Wood costs for Shrine

## v0.1d

+ Make Ancestors look ghostly
+ Make Ancestors completely immune to all forms of attack
+ Disable Ancestor reactions to melee attackers

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

## DONE Hospitality-style visits
+ DONE Ancestors spawn for "visits" instead of constantly
+ DONE Visit times semi-random; normalized to give same approval+/- over time
+ DONE Approval overhaul
  + DONE Approval tracked on a per-visit basis
  + DONE Approval changes submitted when Ancestors conclude visit
  + DONE Delta events (gifts/punishments) occur only on conclusion of visit
  + DONE Magic still granted on season change
+ DONE Interruptions
  + DONE Ancestors vanish (huge approval hit) if shrine destroyed while visiting
  + DONE Ancestors will get super mad if they can't return to their anchor
+ DONE Behaviours
  + DONE Ancestors will occasionally walk outside of colony
  + DONE Berserk Ancestors don't throw exceptions for no attack action)

## DONE Variable Blessings/Punishments
+ DONE Add blessings/punishments instead of cargo pods/flashstorm
+ DONE Create Defs structure for Blessings/Punishments
+ DONE Punishments
  + DONE Small mood penalty for all
  + DONE Large mood penalty for single
  + DONE Minor blight
  + DONE Single-pawn lightning strike
+ DONE Blessings
  + DONE Small mood bonus for all
  + DONE Large mood bonus for single
  + DONE Pregnant (expensive) animal (if easily implementable)

## DONE Beginner Affordances
+ DONE Don't start timer on Shrines until after a certain time period has passed
+ DONE Possibly add in a tutorial message?

## DONE Misc Pre-Release Stuff
+ DONE Ancestors don't have nudist/clothing-based thoughts
+ DONE Weird ThinkTree exception on visits
+ DONE Clean up logging

## DONE Hospitality Compatibility

# This Stuff Would Be Awesome, But Are Not In Scope Right Now
## DONE Visuals
+ DONE Ancestors are colored differently or transparent or something distinct

## Mental Breaks
+ Major: Insult Spree
+ Major: Leave In Anger
+ Minor: Harass colonist
+ Minor: Targeted Insult

## Behaviours
+ Ancestors will hunt down and compliment/insult colonists
+ Ancestors get angry if they can't go outside

## More Blessings 1
  + Minor crop blessing
  + Single-pawn healing

## More Punishments 1
  + Single-pawn sickness

## Offerings
+ Altar at which you can offer things to your Ancestors to boost Approval
+ Requests from Ancestors that you have to fulfill, or lose Approval
+ Specific Ancestors having likes/dislikes in terms of offerings

## Ancestor Events
+ Ancestral visit: many Ancestors manifest for a period of time
+ Quiet dead: Ancestors leave your colony for a period of time

## GODLARP

# Your Code Sucks
+ Stop checking against the literal string "Spirit" to see if you can Do X
+ DONE Use Reflection to set the private weather ticker insteaed of...what you do