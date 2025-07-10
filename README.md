# ASS (Actually Simple SSS)

ASS is a plugin that fundamentally reworks base game ServerSpecificSettings to include better features for:

    1. A queue system to only update a players keybinds while they're not inside the SSS tab leading to less client side
    lag during gameplay

    2. A ASSGroup class designed to handle grouping together settings with a Priority value to help position settings,
    a Predicate of a Player to specify what group of players should see the group, and a SubGroups value to let you stack
    Groups inside each other

    3. A PlayerMenu class designed to handle settings for specific players

    4. Better classes that are easier to read than ServerSpecificSettingBase based objects

    5. Specific events for each type of ASS setting

    6. An IgnoreNextResponse value to help reduce infinite loops in certain cases as well as minimizing accidental
    keybind triggers

    7. A comprehensive SendToPlayerFull method to specify several values indicating how the settings will be handled (Like what settings
    to receive responses from, whether or not to increment the version to show the player a red circle in their tab, etc...)

Inside this repo there is also an ASS.Example plugin showcasing some of the use cases of ASS, to get a better sense of how to use the plugin, consider reviewing its code.


# Does this break SSS?
For most cases, no. But if you have a plugin that uses SSS in a specific way, I can't guarantee compatability. However, if your SSS plugins only modify SSSS.DefinedSettings and call SSSS.SendToPlayer, it should be fine.


# Can you add X/Y/Z?
If you want to add your own features, feel free to fork this repo and edit it from there. However, if you make a PR that is good, I'll merge it if you inform me. You can find more features to add in TODO.txt in the ASS folder


# Please ping me on discord if you find any issues!
(that is unintneded behavior)
My discord is atsomeone btw
