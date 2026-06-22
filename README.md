# Third Eye

View player information without manually clicking on them!

Third Eye lists all players in your current instance and gives you quick-action buttons for each — no need to click on their character in the world.

## What it does

- Lists every player currently in your instance
- For each player, lets you:
  - **Examine** — open their gear inspection sheet
  - **View Adventure Plate** — open their adventurer card
  - **Copy name** — copy their name to clipboard for use in `/tell`
  - **Invite to Party** — send a party invite directly
  - **Send Friend Request** — add them as a friend

## Requirements

- [FFXIV on Mac](https://www.xivonmac.com/) or Windows with [XIVLauncher](https://goatcorp.github.io/)
- [Dalamud](https://github.com/goatcorp/Dalamud) plugin framework (API Level 15)

## Installation

> The custom repo link will be available after the first release is published.

1. Open Dalamud settings (`/xlsettings`)
2. Go to **Experimental** → **Custom Plugin Repositories**
3. Add: `https://raw.githubusercontent.com/bimilbimil/XIVThirdEye/main/repo.json`
4. Search for **Third Eye** in the Plugin Installer and install

## Commands

| Command | Description |
|---|---|
| `/thirdeye` | Open the Third Eye window |

## Usage

1. Enter any instance (dungeon, overworld zone, etc.)
2. Use `/thirdeye` to open the player list
3. Click any action button next to a player's name
