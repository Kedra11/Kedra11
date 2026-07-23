extends Node2D

# ============================================================================
# BOARD GEOMETRY
# ============================================================================
const CELL_SIZE: int = 32
const GRID_COLUMNS: int = 26
const GRID_ROWS: int = 26
const TANK_CELL_SIZE: int = CELL_SIZE * 2
const MOVEMENT_CELL_SIZE: int = CELL_SIZE
const MOVEMENT_GRID_COLUMNS: int = GRID_COLUMNS
const MOVEMENT_GRID_ROWS: int = GRID_ROWS
const DESTRUCTION_SUBDIVISIONS: int = 2
const DESTRUCTION_CELL_SIZE: int = CELL_SIZE / DESTRUCTION_SUBDIVISIONS
const DESTRUCTION_GRID_COLUMNS: int = GRID_COLUMNS * DESTRUCTION_SUBDIVISIONS
const DESTRUCTION_GRID_ROWS: int = GRID_ROWS * DESTRUCTION_SUBDIVISIONS
const BOARD_SIZE: Vector2 = Vector2(GRID_COLUMNS * CELL_SIZE, GRID_ROWS * CELL_SIZE)
const BOARD_ORIGIN: Vector2 = Vector2(224.0, 32.0)
const SCREEN_SIZE: Vector2 = Vector2(1280.0, 896.0)
const PANEL_LEFT_WIDTH: float = 224.0
const PANEL_RIGHT_X: float = 1056.0

# ============================================================================
# GAMEPLAY TUNING
# ============================================================================
const STARTING_LIVES: int = 3
const STAGE_COUNT: int = 35
const MAX_ENEMIES_ON_FIELD: int = 4
const ENEMIES_PER_STAGE: int = 20
const MAX_PLAYER_BULLETS_BASE: int = 1
const MAX_PLAYER_BULLETS_UPGRADED: int = 2
const RESPAWN_INVULNERABLE_TIME: float = 2.8
const TURN_ALIGNMENT_EPSILON: float = 5.0
const TURN_SNAP_DISTANCE: float = 8.0
const TANK_RADIUS: float = 28.5
const TANK_DRAW_SIZE: float = 64.0
const BULLET_RADIUS: float = 5.0
const BULLET_TRACE_STEP: float = 4.0
const BULLET_SPAWN_OFFSET: float = 40.0

# Original cattle-bity speeds (32 px sub-tile, matches our CELL_SIZE).
const PLAYER_MOVE_SPEED: float = 180.0
const ENEMY_SPEED_SLOW: float = 120.0
const ENEMY_SPEED_FAST: float = 240.0
const BULLET_SPEED_SLOW: float = 600.0
const BULLET_SPEED_FAST: float = 900.0
const PLAYER_SHOOT_COOLDOWN_SLOW: float = 0.34
const PLAYER_SHOOT_COOLDOWN_FAST: float = 0.18
const ENEMY_SHOOT_DELAY_MIN: float = 1.1
const ENEMY_SHOOT_DELAY_MAX: float = 2.25
const ENEMY_TURN_DELAY_MIN: float = 0.35
const ENEMY_TURN_DELAY_MAX: float = 1.05
const ENEMY_SPAWN_FLASH_TIME: float = 1.7
const ENEMY_FIRST_SPAWN_DELAY: float = 0.16
const ENEMY_SPAWN_DELAY: float = 3.0
const ICE_SLIDE_TIME: float = 0.40

const POWERUP_LIFETIME: float = 30.0
const SHIELD_DURATION: float = 10.0
const BASE_DEFENCE_DURATION: float = 17.0
const BASE_DEFENCE_FADE_TIME: float = 3.5
const BASE_DEFENCE_FADE_INTERVAL: float = 0.25
const FREEZE_DURATION: float = 10.0
const POWERUP_PICKUP_POINTS: int = 1000
const EXPLOSION_SMALL_TIME: float = 0.30
const EXPLOSION_BIG_TIME: float = 0.55
const SCORE_POPUP_TIME: float = 0.85

# Juice / FX layer (visual & audio polish only — gameplay canon untouched).
const STAGE_INTRO_TIME: float = 1.5
const STAGE_INTRO_OPEN_TIME: float = 0.6
const SFX_RATE: int = 22050
const SFX_POOL_SIZE: int = 12

const EXTRA_LIFE_THRESHOLD: int = 20000
const DEFAULT_HIGHSCORE: int = 20000

# Player upgrade tiers (stars)
const STAR_NONE: int = 0
const STAR_FAST_BULLET: int = 1
const STAR_TWO_BULLETS: int = 2
const STAR_BREAK_STEEL: int = 3

# ============================================================================
# TILE TYPES
# ============================================================================
const TILE_EMPTY: int = 0
const TILE_BRICK: int = 1
const TILE_STEEL: int = 2
const TILE_WATER: int = 3
const TILE_FOREST: int = 4
const TILE_ICE: int = 5

# ============================================================================
# COLORS
# ============================================================================
const COLOR_BG: Color = Color(0.03, 0.03, 0.03, 1.0)
const COLOR_BOARD: Color = Color(0.0, 0.0, 0.0, 1.0)
const COLOR_PANEL: Color = Color(0.48, 0.48, 0.44, 1.0)
const COLOR_BRICK: Color = Color(0.62, 0.31, 0.18, 1.0)
const COLOR_STEEL: Color = Color(0.64, 0.66, 0.66, 1.0)
const COLOR_WATER: Color = Color(0.07, 0.21, 0.58, 1.0)
const COLOR_FOREST: Color = Color(0.08, 0.36, 0.12, 0.78)
const COLOR_ICE: Color = Color(0.62, 0.85, 0.95, 0.55)
const COLOR_PLAYER: Color = Color(0.86, 0.76, 0.28, 1.0)
const COLOR_BASE: Color = Color(0.88, 0.78, 0.45, 1.0)
const COLOR_BULLET: Color = Color(1.0, 0.92, 0.55, 1.0)
const COLOR_ENEMY_BASIC: Color = Color(0.78, 0.78, 0.78, 1.0)
const COLOR_ENEMY_FAST: Color = Color(0.78, 0.30, 0.78, 1.0)
const COLOR_ENEMY_POWER: Color = Color(0.30, 0.55, 0.96, 1.0)
const COLOR_ENEMY_ARMOR_FULL: Color = Color(0.72, 0.22, 0.16, 1.0)
const COLOR_ENEMY_ARMOR_3: Color = Color(0.85, 0.45, 0.20, 1.0)
const COLOR_ENEMY_ARMOR_2: Color = Color(0.95, 0.85, 0.25, 1.0)
const COLOR_ENEMY_ARMOR_1: Color = Color(0.65, 0.65, 0.65, 1.0)
const COLOR_DROP_FLASH: Color = Color(0.95, 0.45, 0.25, 1.0)

# ============================================================================
# FIXED CELLS / SPAWNS
# ============================================================================
const BASE_MACRO_CELLS: Array[Vector2i] = [Vector2i(12, 24), Vector2i(13, 24), Vector2i(12, 25), Vector2i(13, 25)]
const PLAYER_SPAWN_CELL: Vector2i = Vector2i(8, 24)
const PLAYER_SPAWN_CELLS: Array[Vector2i] = [Vector2i(8, 24), Vector2i(16, 24)]
const PLAYER_COLORS: Array[Color] = [Color(0.86, 0.76, 0.28, 1.0), Color(0.30, 0.70, 0.30, 1.0)]
const SPAWN_CELLS: Array[Vector2i] = [Vector2i(0, 0), Vector2i(12, 0), Vector2i(24, 0)]
const SCORE_BY_TIER: Dictionary = {"a": 100, "b": 200, "c": 300, "d": 400}
const ENEMY_HP_BY_TIER: Dictionary = {"a": 1, "b": 1, "c": 1, "d": 4}
const POWERUP_TYPES: Array[String] = ["shield", "upgrade", "wipeout", "life", "defence", "freeze"]

# Game state machine
const STATE_MENU: String = "menu"
const STATE_PLAYING: String = "playing"
const STATE_CONSTRUCTION: String = "construction"
const MENU_OPTIONS: Array[String] = ["1 PLAYER", "2 PLAYERS", "CONSTRUCTION"]
const BRUSH_TILES: Array[int] = [TILE_EMPTY, TILE_BRICK, TILE_STEEL, TILE_WATER, TILE_FOREST, TILE_ICE]
const BRUSH_NAMES: Array[String] = ["EMPTY", "BRICK", "STEEL", "WATER", "FOREST", "ICE"]
const HIGHSCORE_PATH: String = "user://hiscore.cfg"
const CUSTOM_LEVEL_PATH: String = "user://custom_level.json"
const EDITOR_REPEAT_DELAY: float = 0.12

# ============================================================================
# STAGE LAYOUTS (35 original Battle City levels, converted from cattle-bity)
# ============================================================================
const STAGE_LAYOUTS: Array[Array] = [
	[
		"..........................",
		"..........................",
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..BBSSBB..BB..BB..",
		"..BB..BB..BBSSBB..BB..BB..",
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..........BB..BB..",
		"..BB..BB..........BB..BB..",
		"..........BB..BB..........",
		"..........BB..BB..........",
		"BB..BBBB..........BBBB..BB",
		"SS..BBBB..........BBBB..SS",
		"..........BB..BB..........",
		"..........BBBBBB..........",
		"..BB..BB..BBBBBB..BB..BB..",
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..........BB..BB..",
		"..BB..BB..........BB..BB..",
		"..BB..BB..........BB..BB..",
		"..........................",
		"..........................",
	],
	[
		"......SS......SS..........",
		"......SS......SS..........",
		"..BB..SS......BB..BB..BB..",
		"..BB..SS......BB..BB..BB..",
		"..BB........BBBB..BBSSBB..",
		"..BB........BBBB..BBSSBB..",
		"......BB..........SS......",
		"......BB..........SS......",
		"......BB....SS....BBFFBBSS",
		"......BB....SS....BBFFBBSS",
		"..FF......BB....SS..FF....",
		"..FF......BB....SS..FF....",
		"..BBBBBBFFFFFFSS....FFBB..",
		"..BBBBBBFFFFFFSS....FFBB..",
		"......SSFFBB..BB..BB..BB..",
		"......SSFFBB..BB..BB..BB..",
		"SSBB..SS..BB..BB......BB..",
		"SSBB..SS..BB..BB......BB..",
		"..BB..BB..BBBBBB..BBSSBB..",
		"..BB..BB..BBBBBB..BBSSBB..",
		"..BB..BB..BBBBBB..........",
		"..BB..BB..BBBBBB..........",
		"..BB..............BB..BB..",
		"..BB..............BB..BB..",
		"..BB..BB..........BBBBBB..",
		"..BB..BB..........BBBBBB..",
	],
	[
		"........BB......BB........",
		"........BB......BB........",
		"..FFFFFFBB................",
		"..FFFFFFBB..........SSSSSS",
		"BBFFFFFF..................",
		"BBFFFFFF..................",
		"FFFFFFFF......BB..BBBBBBB.",
		"FFFFFFFF......BB..BBBBBBB.",
		"FFFFFFFFBBBBBBBB..BB...B..",
		"FFFFFFFFBBBBBB....BB...B..",
		"FFFFFFFF....BB.........B..",
		"FFFFFFFF....BB.........B..",
		"..FF........SSSSSS....FF..",
		"..FF........SSSSSS....FF..",
		"..................FFFFFFFF",
		"..BB..BB..........FFFFFFFF",
		"BBB..BBBB..BBBBBBBFFFFFFFF",
		"BBB..BBBB..B......FFFFFFFF",
		"..........BB......FFFFFFFF",
		"..........BB..BBBBFFFFFFFF",
		"BB....S.......BBBBFFFFFF..",
		"BB....S...........FFFFFF..",
		"BBBB..S...........FFFFFF..",
		"BBBB..S...........FFFFFF..",
		"SSBBBB............BB......",
		"SSBBBB............BB......",
	],
	[
		"..FFFF................FF..",
		"..FFFF................FF..",
		"FFFF......BBBB..........FF",
		"FFFF....BBBBBBBBBB......FF",
		"FF.....BBBBBBBBBBBBB....SS",
		"FF.....BBBBBBBBBBBBBBB....",
		"SS....BBBBBBBBBBBBBBBBB...",
		"......BBBBBBBBBBBBBBBBB...",
		".....BBB......BBBBBB..B...",
		".....B..........BBBB..B...",
		"WW...B..S...S...BBB.......",
		"WW...B..S...S...BBB.......",
		"....BB..........BBB...WWWW",
		"....BB..BBBB....BBB...WWWW",
		"....BBBBBBBBBBBBBBBB......",
		"....BBBBBBBBBBBBBBBB......",
		"...BBBBBBBBBBBBBBBBBB.....",
		"...BBBBBBBBBBBBBBBBBB.....",
		"..BBBBBBBBBBBBBBBBBBBB....",
		"......BBBBBBBBBBBB........",
		"..BBBB..BBBBBBBB..BBBB..FF",
		"..BBBBBB..BBBB..BBBBBB..FF",
		"FF..BBBB........BBBB..FFFF",
		"FF....................FFFF",
		"SSFF................FFFFSS",
		"SSFF................FFFFSS",
	],
	[
		"........BBBB..............",
		"........BBBB..............",
		"........BB......SSSSSS....",
		"SS......BB..........SS....",
		"SS..BB......BB............",
		"SS..BB......BB............",
		"BB..BBBBBB..BBBB..WWWW..WW",
		"BB..BBBBBB..BBBB..WWWW..WW",
		"BB......BB........WW......",
		"..................WW......",
		"........WWWW..WWWWWW..BBBB",
		"....BB..WWWW..WWWWWW..BBBB",
		"BBBB....WWBB..BBB.........",
		"BBBB....WWBB..BBB.........",
		"........WW...........SS...",
		"........WW...........SS...",
		"WWWWWW..WW..SS..BB...S....",
		"WWWWWW..WW..SS..BB...S....",
		".....................SBBBB",
		"......BBBB...........SBBBB",
		"........BBBBBBBBBB........",
		"........BB......BBBB......",
		"BBBBBB............BBBB....",
		"BBBB................BB....",
		"BB........................",
		"..........................",
	],
	[
		"...........B..B.FFFF......",
		"...........B..B.FFFF......",
		"..B..S..B........BFFB..BFF",
		"..B..S..B........BFFB..BFF",
		"..B..S..B...BB...BFFB..BFF",
		"..B..S..B...BB...BFFB..BFF",
		"..BB....BB..SS..BBFF..BBFF",
		"..BB....BB..SS..BBFF..BBFF",
		".......BSS..BB..BBS...FFFF",
		".......B....BB....S...FFFF",
		"BBBBB.....FFBBFF.....BBBBB",
		"BBBBB.....FFBBFF.....BBBBB",
		".........BFFFFFFB.........",
		".........BFFFFFFB.........",
		"SSBBBB..BBFFFFFFBB.BBBBBSS",
		"SSBBBB....FFFFFF...BBBBBSS",
		"SSSSSS......FF......SSSSSS",
		"........BB..FF..BB........",
		"..BB....BB......BB........",
		"..BB....BB......BB........",
		"..BBB.....BB..BB.....BBBFF",
		"..BBB................BBBFF",
		"....BB..............FFFFFF",
		"....................FFFFFF",
		"......................FFFF",
		"....BB..............BBFFFF",
	],
	[
		"..............SSSS........",
		"..........................",
		"....SSSSSSSS........SS....",
		"....SS..............SS....",
		"....SS......FF..SSSSSS....",
		"....SS......FF....SSSS....",
		"..SS......FFSS......SS....",
		"..SS......FFSS......SS....",
		"........FFSSSS......SSSS..",
		"........FFSSSS........SS..",
		"..SS..FFSSSSSS..SS........",
		"..SS..FFSSSSSS..SS........",
		"...S..SSSS......SSSS......",
		"...S..SSSS......SSSS......",
		"S.......SS..SSSSSS.....S..",
		"S.......SS..SSSSSS.....S..",
		"...SSS......SSSSFF....SS..",
		"...SSS......SSSSFF....SS..",
		"..SS........SSFF....SSSS..",
		"..SS........SSFF....SSSS..",
		"..SSSSSS....FF....SS......",
		"......SS....FF....SS......",
		"..................SS....SS",
		"......................SSSS",
		"..........................",
		"SSSS......................",
	],
	[
		"....BB....BB......BB......",
		"....BB....BB..BB..BB......",
		"FFBBBBBB..BB......BBB.....",
		"FFBBBBBB..BB..SS..BBB.....",
		"FFFFFF....BB..BB..BB...BB.",
		"FFFFFF........BB.......BB.",
		"FFWWWWWWWWWWWWWWWWWWWW..WW",
		"FFWWWWWWWWWWWWWWWWWWWW..WW",
		"..BB......................",
		"..BB........BBBB..........",
		"....BB.....BBBBBBBBBBBSSSS",
		"....BB.....BBBBB..BB......",
		"BBBB..BB...BBBBBFFBB....BB",
		"BBBB..BB...BBBBBFFBBSSSSBB",
		"......SS......FFFFFFFF....",
		"......SS..SS..FFFFFFFF....",
		"WWWW..WWWWWWWWWW..WWWWWWWW",
		"WWWW..WWWWWWWWWW..WWWWWWWW",
		"FFFF...B..................",
		"FFFF...B....BBBB..........",
		"FFFFBB..B......B......BB..",
		"FFFFBB..B......B..SSBBBB..",
		"FF..BB..B.........BB..BB..",
		"FFSSBB..B.............BB..",
		"......................BB..",
		"..................BB......",
	],
	[
		"......BB............FF....",
		"......BB..........SSFF....",
		"BB............FF.SSSS...BB",
		"BB..........SSFF.SSSS...BB",
		"........FF.SSSS...SSFF....",
		"......SSFF.SSSS.....FF....",
		".....SSSS...SSFF..........",
		".....SSSS.....FF..........",
		"......SSFF................",
		"........FF................",
		"......FF..FF..FF..FF......",
		"......FFSSFF..FFSSFF......",
		"SSBB...SSSS....SSSS...BBSS",
		"SSBB...SSSS....SSSS...BBSS",
		"......FFSSFF..FFSSFF......",
		"......FF..FF..FF..FF......",
		"..........................",
		"........SS......SS........",
		"BB.....SSSS....SSSS.....BB",
		"BB.....SSSS....SSSS.....BB",
		"BB....FFSSFF..FFSSFF....BB",
		"BB....FF..FF..FF..FF....BB",
		"..........................",
		"....BB..............BB....",
		"....BBBB..........BBBB....",
		"....BBBB..........BBBB....",
	],
	[
		"..........................",
		"..........................",
		"...BBBBB............BBBBB.",
		"...B..BB............BB..B.",
		".BBB....BB..FFFF..BB.....B",
		".B......BB..FFFF..BB.....B",
		"BB......BBFFFFFFFFBB.....B",
		"BB......BBFFFFFFFFBB.....B",
		"BB.....BBBFFSSSSFFBBB...BB",
		"BB.....BBBFFSSSSFFBBB...BB",
		".B....BBWWWWWWWWWWWWBBBBBB",
		".BBBBBBBWWWWWWWWWWWWBBBBBB",
		"..BBBBBBSSSSBBSSSSBBBBBBB.",
		"..BBBBBBSSSSBBSSSSBBBBBBB.",
		"....BBBBSS..BB..SSBBBBB...",
		"....BBBBSS..BB..SSBBBBB...",
		"....BBBBBBBBBBBBBBBBBBB...",
		"....BBBBBBBBBBBBBBBBBBB...",
		"BBFFBBBBBBSSSSBBBBBBBBFFBB",
		"BBFF......SSSS........FFBB",
		"BBFFFFFFFFFFFFFFFFFFFFFFBB",
		"BBFFFFFFFFFFFFFFFFFFFFFFBB",
		"....FFFFFF......FFFFFFFF..",
		"....FFFFFF......FFFFFFFF..",
		"......B.............B.....",
		"......B.............B.....",
	],
	[
		"..........SS..BB..BBBB....",
		"..........SS..BB..BBBB....",
		"...BBBBBBBBB..BB..........",
		"...BBBBBBBBB..BB..........",
		"......B...BB..BBBB..FFFFFF",
		"......B...BB..BBBB..FFFFFF",
		"...B..........SS..FFFFFFFF",
		"...B..........SS..FFFFFFFF",
		"...B..BBBBBBSSBBBBFFFFBBSS",
		"...B..BBBBBBSSBBBBFFFF..SS",
		"..BBBBBBSS....BB..FFFF...B",
		"........SS....BB..FFFF...B",
		".BBBBBBB..SSFFFFFFFFFF....",
		".BBBBBBB..SSFFFFFFFFFF....",
		"......SS....FFFFFFFFFFBB..",
		"......SS....FFFFFFFFFFBB..",
		"SSBB..FFFFFFFFSSFFFFFFBB..",
		"SSBB..FFFFFFFFSSFFFFFFBB..",
		".BBBFFFFFFFFFF........BBB.",
		".BBBFFFFFFFFFF........BBB.",
		"..BBFFFF........SSBBBBBB..",
		"..BBFFFF..........BBBBBB..",
		"....FFFF..........BB...B..",
		"....FFFF..........BB...B..",
		"....FFFF..................",
		"..BBFFFF..................",
	],
	[
		"..............BBBBBB......",
		"..............BBBBBB......",
		"..BBBBBB..........BB......",
		"..BBBBBBBB..BB....BB......",
		"........BB..BB........BBBB",
		"........BB............BBBB",
		"..WWWWWWWWWW..BBB.....BBSS",
		"..WWWWWWWWWW..BBB.....BB..",
		"..........WW..BB..SSS.BB..",
		"....SSSSSSWW..BB..SSS.BB..",
		"BB..BBBBBBWWWWWW..WWBBBB..",
		"BB..BBBBBBWWWWWW..WWBBBB..",
		"........SSWW......WWSS....",
		"........SSWW......WW......",
		"WWWWWW..WWWWBBBB..WW......",
		"WWWWWW..WWWWBBBB..WW......",
		"..........BBSSSS..WWWWWW..",
		"..........BB......WWWWWW..",
		"BBBBBB....................",
		"BBBBBB....................",
		"....BB..SSSS......BBBB...B",
		"....BB............BBBB...B",
		"BB................BB....BB",
		"BB................BB....BB",
		"..........................",
		"..........................",
	],
	[
		"..........................",
		"........BB......BB........",
		"..BBBBBBBB......BBBBBBBB..",
		"..BBBBBBBB......BBBBBBBB..",
		"..BB........BB........SS..",
		"..BB........BB........SS..",
		"..SS..BBBB......BBBB..BBBB",
		"..SS..BB..........BB..BBBB",
		"..BB..B.FF..SS..FF.B..SSBB",
		"..BB..B.FFSSSSSSFF.B..SSBB",
		"..BB....FFFFFFFFFF....SSBB",
		"........FFFFFFFFFF......BB",
		"BB......FFFFFFFFFF......BB",
		"BBSS....FFFFFFFFFF....BBBB",
		"BBSS..B.FFSSSSSSFF.B..BB..",
		"BBSS..B.FF..SS..FF.B..BB..",
		"BBBB..BB..........BB..SS..",
		"BBBB..BBBB......BBBB..SS..",
		"BBSS........BB........BB..",
		"BBSS........BB........BB..",
		"BBBBBBBBBB......BBBBBBSSSS",
		"BBBBBBBBBB......BBBBBBSSSS",
		"BBBB....BB......BB....BB..",
		"BBBB..................BB..",
		"BBBB......................",
		"BBBB......................",
	],
	[
		"..........................",
		"..........................",
		"FFFF......BBBBBB......FFFF",
		"FFFF....BBBBBBBBBB....FFFF",
		"FF.....BBBBBBBBBBBB.....FF",
		"FF.....BBBBBBBBBBBB.....FF",
		"......BBBBFFBBFFBBBB......",
		"......BBBBFFBBFFBBBB......",
		"......BBFFFFBBFFFFBB......",
		"......BBFFFFBBFFFFBB......",
		"FF....BBBBBBBBBBBBBB....FF",
		"FF....BBBBBBBBBBBBBB....FF",
		"FFFF....BBFFBBFFBB....FFFF",
		"FFFF....BBFFBBFFBB....FFFF",
		"WWWWWW..BBBBBBBBBB..WWWWWW",
		"WWWWWW..BBBBBBBBBB..WWWWWW",
		".........B.B.B.B.B........",
		".........B.B.B.B.B........",
		"........B.B.B.B.B.........",
		"........B.B.B.B.B.........",
		".S.S.S..............S.S.S.",
		".S.S.S..............S.S.S.",
		"B.B.B................B.B.B",
		"B.B.B................B.B.B",
		"S.S.S..S..........S..S.S.S",
		"S.S.S..S..........S..S.S.S",
	],
	[
		"........BBBB....BB........",
		"........BBBB....BB........",
		"..FFFFBBBB......BB........",
		"..FFFFBBBB......BB........",
		"FFFFFFFFFFFFFFFFBBBB......",
		"FFFFFFFFFFFFFFFFBBBB......",
		"FFSSBBFFBBBBBBFFFFFFFFBBSS",
		"FF..BBFFBBBBBBFFFFFFFFBBSS",
		"FFFFBBFFFFFFSSFFFFBBS.BB..",
		"FFFFBBFFFFFF..FFFFBBS.BB..",
		"..FFFFBB..FFFFFFFFBB..BB..",
		"..FFFFBBSSFFFFFFFFBB..BB..",
		"..BBBBBBBBBBFFFFBBBBB.FFFF",
		"..BBBBBBBBBBFFFFBBBBB.FFFF",
		".SSSBBBB......BBBB......FF",
		".S..BBBB......BB........FF",
		"..BB..BB......BBFFFFBBB.FF",
		"..BB..BB..SSBB..FFFFBBB.FF",
		"..BB.....BBBBBFFFFBB....FF",
		"..BB.....BBB..FFFFBB....FF",
		"..BBBBB..BBBFFFFBBFFBBFFFF",
		"..BBBBB..B..FFFFBBFFBBFFFF",
		"....BB..FF......BBFFBBFF..",
		"....BB..FF......BBFF..FF..",
		"....BB............FFFFFF..",
		"..................FFFFFF..",
	],
	[
		"..........................",
		"..........................",
		"....SSFFSS................",
		"....SSFFSS................",
		"......FF..FF..............",
		"......FF..FFSS............",
		"..FF........FF............",
		"..FF........FFBB..........",
		"..FFFF....FF..FF..........",
		"..FFFF....FF..FFSS........",
		"..FF..FF..FF....FF........",
		"..FF..FF..FF....FFBB......",
		"..FF....FF......FFFF......",
		"..FF....FF......FFFFSS....",
		"....FF........FFFFFFFF....",
		"....FF........FFFFFFFFBB..",
		"......FF....FF..FFFFFFFF..",
		"......FF....FF..FFFFFFFF..",
		"BB..........FF....FFFFFFSS",
		"BB..........FF....FFFFFFSS",
		"BBBB..........FF..FFFFFFFF",
		"BBBB..........FF..FFFFFFFF",
		"SSBBBB..........FF..FFFFFF",
		"SSBBBB..........FF..FFFFFF",
		"SSSSBBBB........FF....FFFF",
		"SSSSBBBB........FF....FFFF",
	],
	[
		"..........................",
		"........BB..........BB....",
		"..BB..BBBB....IIIIIIBBBB..",
		"..BB..BBBB....IIIIIIBBBB..",
		"..BB....BB..SSIIIIIIIIII..",
		"..BB....BB..SSIIIIIIIIII..",
		"IIIIIIS.BB....BBIIIIIIII..",
		"IIIIIIS.BB....BBIIIIIIII..",
		"IIIIIIIIIIIIBBBB.BB.......",
		"IIIIIIIIIIIIBBBB.BB.......",
		".....SIIIIIIIIBB.BB...SSSS",
		".....SIIIIIIIIBB.BB.......",
		"BBBBBBBBIIIIIIIIIIIIIIBBBB",
		"BBBBBBBBIIIIIIIIIIIIIIBBBB",
		"......BBBBIIIIIIIIS.......",
		"......BBBBIIIIIIIIS.......",
		"..BBBBBB..IIIIIIBBBB..BB..",
		"..BBBBBB..IIIIIIBBBB..BB..",
		"IIIIIIBBII........BB..BB..",
		"IIIIIIBBII........BB..BB..",
		"IIIIIIIIIISS..SS......BB..",
		"IIIIIIIIII..........BBBB..",
		"BBIIIIIIII........BB......",
		"BBIIIIIIII........BB......",
		"BBBBS.............BB..BB..",
		"BBBBS.............BB..BB..",
	],
	[
		"................SSSSSSFF..",
		"................SSSSSSFF..",
		"..BB............SS....SS..",
		"..BB............SS....SS..",
		"BBFFBB......BBBBBBBB..SS..",
		"BBFFBB......BBBBBBBB..SS..",
		"..BBFFBB....BB..FFBBSSSS..",
		"..BBFFBB....BB..FFBBSSSS..",
		"....BB..FFSSBBFF..BB......",
		"....BB..FFSSBBFF..BB......",
		"........SS..BBSSBBBB......",
		"........SS..BBSSBBBB......",
		"....BBBBSSBB..SS..........",
		"....BBBBSSBB..SS..........",
		"....BB..FFBBSSFF..........",
		"....BB..FFBBSSFF..........",
		"SSSSSSFF..BB....BBBB......",
		"SSSSSSFF..BB....BBBB......",
		"SS..BBBBBBBB....BBSSSS....",
		"SS..BBBBBBBB....BBSSSS....",
		"SS....SS..........SSBBBB..",
		"SS....SS..........SSBBBB..",
		"FFSSSSSS............BBSSSS",
		"FFSSSSSS............BBSSSS",
		"......................SSSS",
		"......................SSSS",
	],
	[
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..BB..BB..BB..BB..",
		"..BB..BB..BB..BB..BB..BB..",
		"..SS..SS..SS..SS..SS..SS..",
		"..........................",
		"........BB......BB........",
		"BB..BB..BB......BB..BB..BB",
		"BB..BBBBBB..BB..BBBBBB..BB",
		"BB..BB..BB..BB..BB..BB..BB",
		"SS..SS..SS..SS..SS..SS..SS",
		"........SS......SS........",
		"FFFF....BB..FF..BB....FFFF",
		"FFFF....BB..FF..BB....FFFF",
		"FFFFFFFFBBBBFFBBBBFFFFFFFF",
		"FFFFFFFFBB..FF..BBFFFFFFFF",
		"FFFFFFFFFFFFFFFFFFFFFFFFFF",
		"FFFFFFFFFFFFFFFFFFFFFFFFFF",
		"........BBFFFFFFBB........",
		"BB..BB..BBFFFFFFBB..BB..BB",
		"..BB..BB....FF....BB..BB..",
		"..BB..BB....FF....BB..BB..",
		"..BB..BB..........BB..BB..",
		"..BB..BB..........BB..BB..",
		"..BB..BB..........BB..BB..",
		"..........................",
	],
	[
		"......WW..BB....BB..BB....",
		"......WW..BB....BB..BB....",
		"................BB..SS....",
		"................BB..SS....",
		"......WW....SS..BB..BB....",
		"......WW..BBSS..BB..BB....",
		"SS..BBWW..SS....BB..BB....",
		"....BBWW..SS..BB....BB....",
		"....BBWW......BB..........",
		"....BBWW......BB..........",
		"BB..BBWWWW..WWWWWWWW....BB",
		"BB..BBWWWW..WWWWWWWW....BB",
		"..............FF..WW..SSSS",
		"......BB......FF..WW......",
		"BBBB.BBB..SSFFFFFFWW......",
		"BBBB.BBB..SSFFFFFFWW..BBBB",
		"BB...B....BBFFFFFFWW..BB..",
		".....B....BBFFFFFFWW..BB..",
		"..........BB..FF..WW..FF..",
		"..SS......BB..FF..WW..FF..",
		"..BB......BBBBBB....FFFFFF",
		"..BB..SS............FFFFFF",
		"..BB..BB..........WWFFFFFF",
		"..BB..BB..........WWFFFFFF",
		"..................WW..FF..",
		"..................WW..FF..",
	],
	[
		"..........................",
		"......BBBBBB....BB........",
		"....BBBBBBBBBBBBBBBB......",
		"..BBBBBBBBBBBBBBBBBB......",
		"..FFFFFFFFFFFFFFFFBBBB....",
		"..FFFFFFFFFFFFFFFFBBBB....",
		"FFFF............FFFFBBBB..",
		"FFFF............FFFFBBBB..",
		"FF..SS....SS......FFFFFF..",
		"FF..SS....SS......FFFFFF..",
		"FF..SS....SS......FFFFFF..",
		"FF..SS....SS......FFFFFF..",
		"FF....FF........FFFFBBBBB.",
		"FF....FF........FFFFBBBBB.",
		"FFFFFFFFFFFFFFFFFFBBBBBBB.",
		"FFFFFFFFFFFFFFFFFFBBBBBBB.",
		"BBFFFFBBBBFFFFFFBBBBBBBB..",
		"BBFFFFBBBBFFFFFFBBBBBBBB..",
		"..BBBBBBBBBBBBBBBBBBBB..SS",
		"..BBBBBBBBBBBBBBBBBBBB..SS",
		"SS..BBSSBBBBBBBBBBBBB...SS",
		"SS..BBSSBBBBBBBBBBBBB...SS",
		"..SSBB..SS......BBBBSSSSSS",
		"..SSBB..SS......BBBBSSSSSS",
		"..........................",
		"..........................",
	],
	[
		"..........FF..............",
		"..........FF..............",
		"........FFSSFF............",
		"........FFSSFF............",
		"....FF....FF....FFFF......",
		"....FF....FF....FFFF......",
		"..FFBBFF......FFBBBBFF....",
		"..FFBBFF......FFBBBBFF....",
		"....FFBBFF......FFFF....FF",
		"....FFBBFF......FFFF....FF",
		"FF....FF....FF........FFSS",
		"FF....FF....FF........FFSS",
		"BBFF......FFSSFF....FF..FF",
		"BBFF......FFSSFF....FF..FF",
		"SSBBFF......FF....FFSSFF..",
		"SSBBFF......FF....FFSSFF..",
		"BBFF....FF......FF..FF....",
		"BBFF....FF......FF..FF....",
		"FF....FFBBFF..FFBBFF......",
		"FF....FFBBFF..FFBBFF......",
		"......FFBBFF....FF....FF..",
		"......FFBBFF....FF....FF..",
		"..FF....FF..........FFSSFF",
		"..FF....FF..........FFSSFF",
		"FFSSFF............FFBBFF..",
		"FFSSFF............FFBBFF..",
	],
	[
		"..........................",
		"..........................",
		"..........SSSS............",
		"..........SSSS............",
		"............SS............",
		"............SS............",
		"..SSSSFFFFBBSSBBFFFFSSSS..",
		"..SSSSFFFFBBSSBBFFFFSSSS..",
		"......SSFFFFSSFFFFSS......",
		"......SSFFFFSSFFFFSS......",
		"FF......SSFFFFFFSS......FF",
		"FF......SSFFFFFFSS......FF",
		"SSFF......FFFFFF......FFSS",
		"SSFF......FFFFFF......FFSS",
		"FF........SSFFSS........FF",
		"FF......SS..FF..SS......FF",
		"........SS......SS........",
		"........SS..SS..SS........",
		"......SS....SS....SS......",
		"......SS....SS....SS......",
		"..........................",
		"..........................",
		"..........................",
		"..........................",
		"....SS..............SS....",
		"....SS..............SS....",
	],
	[
		"....SS..BBSS.........B....",
		"....SS..BB...........B....",
		"....BB..BBFF..BBBBBBBB....",
		"....BB..BBFF....BBBBBB....",
		"..FFFF..BBFF.BB.......SSSS",
		"..FFFF..BBFF.BB.......SSSS",
		"FFFFFFFFFFFFBBBBBB...BBB..",
		"FFFFFFFFFFFFBBBBBB...BBB..",
		"....FFFF....SSBB...BBBBB.B",
		"....FFFFBBBB..BB...BBB...B",
		"BBSS....BBBB......BBBB...B",
		"BB....BB..........BB.....B",
		".B....BBIIIIIIIIIIIIIIIIII",
		".B..BBBBIIIIIIIIIIIIIIIIII",
		".B..BB..IIIIIIIIIIIIIIIIII",
		".B......IIIIIIIIIIIIIIIIII",
		"....SS..IIIIIIIIIIIIIIIIII",
		"....SS..IIIIIIIIIIIIIIIIII",
		"BB..BB..IIIIIIIIIIIIIIIIII",
		"BB..BB..IIIIIIIIIIIIIIIIII",
		".B..BB..IIIIIIIIIIIIIIIIII",
		".B..BB..IIIIIIIIIIIIIIIIII",
		".B..BB..........IIIIIIIIII",
		".B..BB..........IIIIIIIIII",
		"....BB............IIIIIIII",
		"..................IIIIIIII",
	],
	[
		"......SS..BB..BB..BB..SS..",
		"......SS..BB..BB..BB..SS..",
		"..BB..BB..........SS......",
		"..BB..BB..........SS......",
		"..BB..BB....SS....SS..SSSS",
		"..BB..BB....SS....SS..SSSS",
		"..BB......BB..SSBB......SS",
		"..BB......BB..SSBB......SS",
		"........BBBB..BBBB..SS....",
		"........BBBB..BBBB..SS....",
		"....SS..BB....BBBB..BBBB..",
		"....SS..BB....BBBB..BBBB..",
		"SS..SS....BB..SS....SSBB..",
		"SS..SS....BB..SS....SSBB..",
		"....BBBB..BB......BBSS....",
		"....BBBB..BB......BBSS....",
		"..SSBBBB..BBBB..BBBB....BB",
		"..SSBBBB..BBBB..BBBB....BB",
		"..BB......BBSS........BBBB",
		"..BB......BBSS........BBBB",
		"......BB..BBBBSS..SS....BB",
		"......BB..BBBBSS..SS....BB",
		"BB..BBBB..........BBSS....",
		"BB..BBBB..........BBSS....",
		"BB..BB............BBBBBB..",
		"BB..BB............BBBBBB..",
	],
	[
		"....WWWW..................",
		"....WWWW..................",
		"......WWFF..B.............",
		"SS....WWFF..B.............",
		"FF..........S...B...WWWW..",
		"FFSS........S...B...WWWW..",
		"FFFF..SS...B....S.FFWW....",
		"FFFF....BB.B....S.FFWW....",
		"FFFFFF....SS...B..........",
		"FFFFFF....SSBB.B........SS",
		"FFFFSS.....B..SS........FF",
		"FFFF..SS...B..SSBB....SSFF",
		"FFSS....BBSS...B..SS..FFFF",
		"FF........SS...B......FFFF",
		"..........B.BBSS....FFFFFF",
		"..........B...SS....FFFFFF",
		"....WWFF.S....B.BB..SSFFFF",
		"....WWFF.S....B...SS..FFFF",
		"..WWWW...B...S..........FF",
		"..WWWW...B...S..........FF",
		".............B..FFWW....SS",
		".............B..FFWW......",
		"SS................WWWW....",
		"SS................WWWW....",
		"SSSS....................SS",
		"SSSS....................SS",
	],
	[
		"........SS................",
		"........SS................",
		"SSSS....SS....SSSS........",
		"SSSS....SS....SSSS........",
		"..SS....SS......SS..SSSSFF",
		"..SS....SS......SS..SSSSFF",
		"..SS....SSSSSS..FF..SS....",
		"..SS....SSSSSS..FF..SS....",
		"..BB........SS..SSSSSS....",
		"..BB........SS..SSSSSS....",
		"FFSSSS..SSBBSSBBBB........",
		"FFSSSS..SSBBSSBBBB........",
		"....SSFFSSFF....BB....SSSS",
		"....SSFFSSFF....BB....SSSS",
		"....SS....FF....SS....SS..",
		"....SS....FF....SS....SS..",
		"....BB....SS....SSSSBBSS..",
		"....BB....SS....SSSSBBSS..",
		"FFSSSSSSFFFFBBSSSS..BBSS..",
		"FFSSSSSSFFFFBBSSSS..BBSS..",
		"......BB........FFFF..BB..",
		"......BB........FFFF..BB..",
		"......SS..........FF..BB..",
		"......SS..........FF..BB..",
		"......SS..........SS..BB..",
		"......SS..........SS..BB..",
	],
	[
		".....................SS...",
		".....................SS...",
		"....................SS....",
		"............SS......SS....",
		"............FF....BBB.....",
		"..........BBFFBB..BBB.....",
		"..........FFFFFF..BBB.....",
		"........SSFFFFFFSSBBB.....",
		"........FFFFIIFFFFBBB.....",
		"......BBFFFFIIFFFFBBB.....",
		"......FFFFIIIIIIFFFFB.....",
		"....SSFFFFIIIIIIFFFFB.....",
		"....FFFFIIIIIIIIIIFFFF....",
		"..BBFFFFIIIIIIIIIIFFFFBB..",
		"..FFFFIIIIIIIIIIIIIIFFFF..",
		"SSFFFFIIIIIIIIIIIIIIFFFFSS",
		"FFFFIIIIIIIIIIIIIIIIIIFFFF",
		"FFFFIIIIIIIIIIIIIIIIIIFFFF",
		"..FFIIIIIIIIIIIIIIIIIIFF..",
		"..FFIIIIIIIIIIIIIIIIIIFF..",
		"..FFIIIIIIIIIIIIIIIIIIFF..",
		"..FFIIIIIIIIIIIIIIIIIIFF..",
		"..FFIIIIII......IIIIIIFF..",
		"..FFIIIIII......IIIIIIFF..",
		"..FFIIII..........IIIIFF..",
		"..FFIIII..........IIIIFF..",
	],
	[
		"....................BB....",
		"....................BB....",
		"..BBWWWW..SS..BB..........",
		"..BBWWWW..SS..BB..........",
		"....WWWWBBFFFFFFWWWW..SS..",
		"....WWWWBBFFFFFFWWWW..SS..",
		"..........FFFFFFWWWWBB....",
		"..........FFFFFFWWWWBB....",
		"..SS....WWWW..FF..........",
		"..SS....WWWW..FF..........",
		"FFFFBB..WWWWSS........BB..",
		"FFFFBB..WWWWSS........BB..",
		"FFFFFF............SS....SS",
		"FFFFFF............SS....SS",
		"..BBWWWW..BB..............",
		"..BBWWWW..BB..............",
		"SS..WWWWFFFFWWWWFFFF..BB..",
		"SS..WWWWFFFFWWWWFFFF..BB..",
		"........FF..WWWWFFFFWWWW..",
		"........FF..WWWWFFFFWWWW..",
		"......SSFF......FFFFWWWW..",
		"......SSFF......FFFFWWWW..",
		"....BB..BB................",
		"....BB..BB................",
		"BB................BBSS....",
		"BB................BBSS....",
	],
	[
		"..........................",
		"..........................",
		"..........................",
		"..........BBBB......SS....",
		"..........FFFF......FF....",
		"..SSSS..BBFFFFSS..BBFFBB..",
		"..FFFF..FFFFFFFF..FFFFFF..",
		"BBFFFFBBFFFFFFFFBBFFFFFFBB",
		"FFFFFFFFFFFFFFFFFFFFFFFFFF",
		"FFFFFFFFFFFFFFFFFFFFFFFFFF",
		"SSFFWWFFFFFFFFFFWWFFFFFFFF",
		"SSFFWWFFFFFFFFFFWWFFFFFFFF",
		"FFFFWWWWWWFFFFFFWWWWWWFFSS",
		"FFFFWWWWWWFFFFFFWWWWWWFFSS",
		"FFFFFFFFWWFFSSFFFFFFWWFFFF",
		"FFFFFFFFWWFFSSFFFFFFWWFFFF",
		"FFFFFFFFFFFFFFFFFFFFFFFFFF",
		"FFFFFFFFFFFFFFFFFFFFFFFFFF",
		"FFFFFFFFFFBBBBFFFFFFFFFFSS",
		"FFFFFFFFFF....FFFFFFFFFF..",
		"SSFFFFFFBB....BBFFFFFFSS..",
		"..FFFFFF........FFFFFF....",
		"..SSBBBB........BBBBBB....",
		"..........................",
		"..........................",
		"..........................",
	],
	[
		"......WW........WW........",
		"......WW........WW........",
		"WWWW..WW..WWWWWWWW..WWWWWW",
		"WWWW..WW..WWWWWWWW..WWWWWW",
		"FFFFBB....BB....WW..WWFFWW",
		"FFFFBB....BB....WW..WWFFWW",
		"FFWWWWWWWW..SS....BBFFFFFF",
		"FFWWWWWWWW..SS....BBFFFFFF",
		"FFFF..WW....WW..WWWWWWWWFF",
		"FFFF..WW....WW..WWWWWWWWFF",
		"WWWW..WW..WWWW....WW......",
		"WWWW..WW..WWWW....WW......",
		"....BBFFBB..BBFF..WW....WW",
		"....BBFFBB..BBFF..WW....WW",
		"..WWWWFFWWWWWWWW..FFBB..WW",
		"..WWWWFFWWWWWWWW..FFBB..WW",
		"BB....BB....WW..FFFFWW..WW",
		"BB....BB....WW..FFFFWW..WW",
		"WWWW..WWWW..WWBBWWWWWW....",
		"WWWW..WWWW..WWBBWWWWWW....",
		"....BB..FFFF....FFWW....WW",
		"....BB..FFFF....FFWW....WW",
		"..WWWWWWFF........WW..WWWW",
		"..WWWWWWFF........WW..WWWW",
		"..WW......................",
		"..WW......................",
	],
	[
		"..IIIIIIIIII..IIIIIIIIII..",
		"..IIIIIIIIII..IIIIIIIIII..",
		"IIIIIIIIIIIIIIIIIIIIIIIIII",
		"IIIIIIIIIIIIIIIIIIIIIIIIII",
		"IIIIIIBBIIIIIIIIIIBBIIIIII",
		"IIIIIIBBIIIIIIIIIIBBIIIIII",
		"IIBB..BB..BBIIBB..BB..BBII",
		"IIBB..BB..BBIIBB..BB..BBII",
		"IIBBBBBB..........BBBBBBII",
		"II....BB..........BB....II",
		"IIIIIIBB..BBSSBB..BBIIIIII",
		"IIIIIIBBBBBBSSBBBBBBIIIIII",
		"SSIIIIII..SS..SS..IIIIIISS",
		"SSIIIIII..........IIIIIISS",
		"IIIIIIII..........IIIIIIII",
		"IIIIIIII..BB..BB..IIIIIIII",
		"IIIIIIII..BB..BB..IIIIIIII",
		"IIIIIIII..BB..BB..IIIIIIII",
		"IIIIIIBB..........BBIIIIII",
		"IIIIIIBB....BB....BBIIIIII",
		"IIBBIIBB..SSSSSS..BBIIBBII",
		"IIBBIIBB..........BBIIBBII",
		"..BB..BB..........BB..BB..",
		"..BBBBBB..........BBBBBB..",
		"..BB..................BB..",
		"..........................",
	],
	[
		"........SS........SS......",
		"........SS........SS......",
		"..SS......SS....SSFFFF....",
		"..SS......SS....SSFFFF....",
		"....SS........SSFF..S.....",
		"....SS........SSFFSSS.....",
		"......SS..FFFFFFFFFF...S..",
		"......SS..FFFFFFFFFF...S..",
		"..S.....SSFFFFSSFF....SS..",
		"..S.....SSFFFFSSFF....SS..",
		"..SSS.FF..SSFFFFSS.....S..",
		"....S.FF..SSFFFFSS.....S..",
		"....FFFFFFFFFF....SS......",
		"....FFFFFFFFFF....SS......",
		"....S.FF..SSFF......SS....",
		"..SSS.FF..SSFF......SS....",
		"..FFFFFFSS..SS........SS..",
		"..FFFFFFSS..SS..SS....SS..",
		"FFFFFFSS.........S........",
		"FFFFFFSS.........S........",
		"....SS.................SSS",
		"....SS.................SSS",
		"....................S.....",
		"..................SSS.....",
		"S.........................",
		"S...SS....................",
	],
	[
		"........B..B..............",
		"........B..B..............",
		"B.B.B..B..B.....B.B.......",
		"B.B.B..B..B.....B.B.......",
		"B.B.B.BBBB......B.BBB.....",
		"B.B.B.BBBB......B.BBB.....",
		".B.B..BBB......BB.BBBB....",
		".B.B..BBB......BB.BBBB....",
		"..B...BB.BB...B.BBBBBB....",
		"..B...BB.BB...B.BBBBBB....",
		"..B..B....BBB.BB.B.BBB....",
		"..B..B....BBB.BB.B.BBB....",
		"..B......BBBBBB...B.BB....",
		"..B......BBBBBB...B.BB....",
		"...B....B.BBBBB...B.BB....",
		"...B....B.BBBBB...B.BB....",
		"...BBBBB..BBBBBB..B.B.BBBB",
		"...B......BBBBBB..B.B...BB",
		"...B.....BB.BB.BB.B.B..B.B",
		"...B.....BB.BB.BB.B.B..B.B",
		"....B...BB.BB.BBBB.....B..",
		"....B...BB.BB.BBBB.....B..",
		"....B..BB.......BBB...B...",
		"....B..BB.......BBB...B...",
		"....B..B..........BBBB....",
		"....B..B..........BBBB....",
	],
	[
		"..........................",
		"..........................",
		"........BB..BB............",
		"........BB..BB............",
		"FF....FFBBFFBBFF....FF....",
		"FF....FFBBFFBBFF....FF....",
		"BBFFFFBBBBBBBBBBFFFFBBFF..",
		"BBFFFFBBBBBBBBBBFFFFBBFF..",
		"BBBBBBBBSSBBSSBBBBBBBBFF..",
		"BBBBBBBBSSBBSSBBBBBBBBFF..",
		"WWWWWWBBBBBBBBBBWWWWWWFF..",
		"WWWWWWBBBBBBBBBBWWWWWWFF..",
		"WWBBBBBBBBBBBBBBBBBBWWWWFF",
		"WWBBBBBBBBBBBBBBBBBBWWWWFF",
		"BBBBBBWWBBBBBBWWBBBBBBFFFF",
		"BBBBBBWWBBBBBBWWBBBBBBFFFF",
		"BBBBWWWWWWBBWWWWWWBBBBWWWW",
		"BBBBWWWWWWBBWWWWWWBBBBWWWW",
		"FFWWWWFFFFFFFFFFWWWWFFWWFF",
		"FFWWWWFFFFFFFFFFWWWWFFWWFF",
		"..FFFF..........FFFF..FF..",
		"..FFFF..........FFFF..FF..",
		"..........................",
		"..........................",
		"..........................",
		"..........................",
	],
]

# Per-stage enemy spawn order (20 enemies). Tiers: a Basic, b Fast, c Power, d Armor.
const STAGE_ENEMY_TIERS: Array[Array] = [
	["a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","b","b"],
	["d","d","b","b","b","b","a","a","a","a","a","a","a","a","a","a","a","a","a","a"],
	["a","a","a","a","a","a","a","a","a","a","a","a","a","a","b","b","b","b","d","d"],
	["c","c","c","c","c","c","c","c","c","c","b","b","b","b","b","a","a","d","d","d"],
	["c","c","c","c","c","d","d","a","a","a","a","a","a","a","a","b","b","b","b","b"],
	["c","c","c","c","c","c","c","b","b","a","a","a","a","a","a","a","a","a","d","d"],
	["a","a","a","b","b","b","b","c","c","c","c","c","c","a","a","a","a","a","a","a"],
	["c","c","c","c","c","c","c","d","d","b","b","b","b","a","a","a","a","a","a","a"],
	["a","a","a","a","a","a","b","b","b","b","c","c","c","c","c","c","c","d","d","d"],
	["a","a","a","a","a","a","a","a","a","a","a","a","b","b","c","c","c","c","d","d"],
	["b","b","b","b","b","d","d","d","d","d","d","c","c","c","c","b","b","b","b","b"],
	["c","c","c","c","c","c","c","c","b","b","b","b","b","b","d","d","d","d","d","d"],
	["c","c","c","c","c","c","c","c","b","b","b","b","b","b","b","b","d","d","d","d"],
	["c","c","c","c","c","c","c","c","c","c","b","b","b","b","d","d","d","d","d","d"],
	["a","a","b","b","b","b","b","b","b","b","b","b","d","d","d","d","d","d","d","d"],
	["a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","b","b","d","d"],
	["d","d","b","b","c","c","c","c","c","c","c","c","a","a","a","a","a","a","a","a"],
	["d","d","d","d","a","a","c","c","c","c","c","c","b","b","b","b","b","b","b","b"],
	["b","b","b","b","d","d","d","d","d","d","d","d","a","a","a","a","c","c","c","c"],
	["b","b","b","b","b","b","b","b","a","a","c","c","d","d","d","d","d","d","d","d"],
	["c","c","c","c","c","c","c","c","b","b","a","a","a","a","a","a","d","d","d","d"],
	["b","b","b","b","b","b","b","b","a","a","a","a","a","a","c","c","d","d","d","d"],
	["d","d","d","d","d","d","c","c","c","c","b","b","b","b","b","b","b","b","b","b"],
	["c","c","c","c","d","d","b","b","b","b","a","a","a","a","a","a","a","a","a","a"],
	["c","c","b","b","b","b","b","b","b","b","d","d","d","d","d","d","d","d","d","d"],
	["b","b","b","b","b","b","d","d","d","d","d","d","a","a","a","a","a","c","c","c"],
	["c","c","d","d","d","d","d","d","d","d","b","b","b","b","b","b","b","b","a","a"],
	["b","b","d","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","c","c"],
	["c","c","c","c","c","c","c","c","c","c","b","b","b","b","d","d","d","d","d","d"],
	["a","a","a","a","b","b","b","b","b","b","b","b","c","c","c","c","d","d","d","d"],
	["c","c","c","b","b","b","b","b","b","b","b","d","d","d","d","d","d","c","c","c"],
	["d","d","d","d","d","d","d","d","a","a","a","a","a","a","c","c","b","b","b","b"],
	["b","b","b","b","d","d","d","d","d","d","d","d","c","c","c","c","b","b","b","b"],
	["c","c","c","c","b","b","b","b","b","b","b","b","b","b","d","d","d","d","d","d"],
	["c","c","c","c","b","b","b","b","b","b","d","d","d","d","d","d","d","d","d","d"],
]

# Indices (0-19) of enemies that flash and drop a power-up when killed.
const STAGE_ENEMY_DROP_INDICES: Array[Array] = [
	[3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17], [2, 10, 17],
	[3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17],
	[3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17],
	[3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17],
	[3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17],
	[3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17],
	[3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17], [3, 10, 17],
]


# ============================================================================
# INNER CLASSES
# ============================================================================

class Bullet:
	var position: Vector2
	var direction: Vector2i
	var speed: float
	var owner_party: String
	var wall_damage_high: bool

	func _init(spawn_position: Vector2, spawn_direction: Vector2i, bullet_speed: float, party: String, high_damage: bool) -> void:
		position = spawn_position
		direction = spawn_direction
		speed = bullet_speed
		owner_party = party
		wall_damage_high = high_damage


class EnemyTank:
	var position: Vector2
	var direction: Vector2i
	var tier: String
	var hp: int
	var move_speed: float
	var bullet_speed: float
	var move_timer: float
	var shoot_timer: float
	var is_drop_carrier: bool
	var is_frozen: bool
	var hit_flash: float

	func _init(spawn_position: Vector2, enemy_tier: String, drop_carrier: bool) -> void:
		position = spawn_position
		direction = Vector2i.DOWN
		tier = enemy_tier
		hp = int(ENEMY_HP_BY_TIER[enemy_tier])
		is_drop_carrier = drop_carrier
		is_frozen = false
		hit_flash = 0.0
		move_timer = randf_range(0.3, 0.9)
		shoot_timer = randf_range(0.9, 2.1)
		match enemy_tier:
			"b":
				move_speed = ENEMY_SPEED_FAST
				bullet_speed = BULLET_SPEED_SLOW
			"c":
				move_speed = ENEMY_SPEED_SLOW
				bullet_speed = BULLET_SPEED_FAST
			_:
				move_speed = ENEMY_SPEED_SLOW
				bullet_speed = BULLET_SPEED_SLOW


class SpawnFlash:
	var position: Vector2
	var time_left: float
	var pending_tier: String
	var pending_drop: bool

	func _init(spawn_position: Vector2, tier: String, drop_carrier: bool) -> void:
		position = spawn_position
		time_left = ENEMY_SPAWN_FLASH_TIME
		pending_tier = tier
		pending_drop = drop_carrier


class PowerupItem:
	var position: Vector2
	var type: String
	var blink_timer: float
	var lifetime: float

	func _init(spawn_position: Vector2, powerup_type: String) -> void:
		position = spawn_position
		type = powerup_type
		blink_timer = 0.0
		lifetime = POWERUP_LIFETIME


class Explosion:
	var position: Vector2
	var time_left: float
	var lifetime: float
	var is_big: bool

	func _init(spawn_position: Vector2, big: bool) -> void:
		position = spawn_position
		is_big = big
		lifetime = EXPLOSION_BIG_TIME if big else EXPLOSION_SMALL_TIME
		time_left = lifetime


class ScorePopup:
	var position: Vector2
	var text: String
	var time_left: float

	func _init(spawn_position: Vector2, popup_text: String) -> void:
		position = spawn_position
		text = popup_text
		time_left = SCORE_POPUP_TIME


class Particle:
	var position: Vector2
	var velocity: Vector2
	var time_left: float
	var lifetime: float
	var color: Color
	var size: float


class PlayerSlot:
	var index: int
	var spawn_cell: Vector2i
	var color: Color
	var position: Vector2
	var direction: Vector2i
	var queued_turn: Vector2i
	var shoot_cooldown: float
	var invulnerable_timer: float
	var lives: int
	var stars: int
	var score: int
	var alive: bool
	var ice_dir: Vector2i
	var ice_time_left: float
	var extra_life_awarded: bool

	func _init(idx: int) -> void:
		index = idx
		spawn_cell = PLAYER_SPAWN_CELLS[idx]
		color = PLAYER_COLORS[idx]
		direction = Vector2i.UP
		queued_turn = Vector2i.ZERO
		shoot_cooldown = 0.0
		invulnerable_timer = 0.0
		lives = STARTING_LIVES
		stars = STAR_NONE
		score = 0
		alive = true
		ice_dir = Vector2i.ZERO
		ice_time_left = 0.0
		extra_life_awarded = false


# ============================================================================
# GAME STATE
# ============================================================================
var _game_state: String = STATE_MENU
var _menu_index: int = 0
var _two_players: bool = false
var _players: Array[PlayerSlot] = []
var _stage_index: int = 0

var _high_score: int = DEFAULT_HIGHSCORE

var _spawned_enemies: int = 0
var _kills: int = 0
var _kills_by_tier: Dictionary = {"a": 0, "b": 0, "c": 0, "d": 0}
var _enemy_spawn_timer: float = 0.0
var _enemy_spawn_index: int = 0

var _base_alive: bool = true
var _base_defence_time_left: float = 0.0
var _base_defence_fade_timer: float = 0.0
var _base_defence_steel_active: bool = false

var _freeze_time_left: float = 0.0

var _game_over: bool = false
var _game_over_progress: float = 0.0
var _stage_clear: bool = false
var _bonus_screen_time: float = 0.0
var _paused: bool = false

var _tiles: Array[Array] = []
var _enemies: Array[EnemyTank] = []
var _bullets: Array[Bullet] = []
var _spawn_flashes: Array[SpawnFlash] = []
var _powerups: Array[PowerupItem] = []
var _explosions: Array[Explosion] = []
var _score_popups: Array[ScorePopup] = []
var _particles: Array[Particle] = []

var _shake_time_left: float = 0.0
var _shake_duration: float = 0.001
var _shake_strength: float = 0.0
var _stage_intro_time_left: float = 0.0

var _sfx_streams: Dictionary = {}
var _sfx_players: Array[AudioStreamPlayer] = []

# Construction Mode
var _editor_layout: Array[String] = []
var _editor_cursor: Vector2i = Vector2i.ZERO
var _editor_brush_index: int = 1
var _editor_repeat_timer: float = 0.0
var _editor_message: String = ""
var _editor_message_time: float = 0.0
var _has_custom_layout: bool = false
var _menu_repeat_timer: float = 0.0

@onready var _status_label: Label = $StatusLabel


# ============================================================================
# LIFECYCLE
# ============================================================================
func _ready() -> void:
	randomize()
	_setup_sfx()
	_load_high_score()
	_load_custom_layout()
	_enter_menu()


func _physics_process(delta: float) -> void:
	match _game_state:
		STATE_MENU:
			_update_menu(delta)
		STATE_CONSTRUCTION:
			_update_construction(delta)
		_:
			_update_play(delta)
	queue_redraw()


func _update_play(delta: float) -> void:
	if Input.is_action_just_pressed("ui_cancel"):
		if _game_over or _stage_clear:
			_enter_menu()
			return
		_paused = not _paused

	if Input.is_action_just_pressed("ui_accept"):
		if _stage_clear and _bonus_screen_time <= 0.0:
			_advance_stage()
			return
		if _game_over:
			_enter_menu()
			return

	if _paused:
		return

	_shake_time_left = maxf(0.0, _shake_time_left - delta)

	if _stage_intro_time_left > 0.0:
		_stage_intro_time_left = maxf(0.0, _stage_intro_time_left - delta)
		return

	for player: PlayerSlot in _players:
		if player.alive and _player_pressed_fire(player):
			_player_shoot(player)

	if _game_over:
		_game_over_progress += delta
		_update_explosions(delta)
		_update_particles(delta)
		return

	if _stage_clear:
		_bonus_screen_time = maxf(0.0, _bonus_screen_time - delta)
		_update_explosions(delta)
		_update_particles(delta)
		return

	for player: PlayerSlot in _players:
		player.shoot_cooldown = maxf(0.0, player.shoot_cooldown - delta)
		player.invulnerable_timer = maxf(0.0, player.invulnerable_timer - delta)
	_freeze_time_left = maxf(0.0, _freeze_time_left - delta)
	_update_base_defence(delta)
	for player: PlayerSlot in _players:
		_update_player(player, delta)
	_update_enemies(delta)
	_update_bullets(delta)
	_update_spawn_flashes(delta)
	_update_powerups(delta)
	_update_explosions(delta)
	_update_particles(delta)
	_update_score_popups(delta)
	_spawn_enemies_if_needed(delta)
	_check_stage_clear()
	_update_status()


func _draw() -> void:
	draw_rect(Rect2(Vector2.ZERO, SCREEN_SIZE), COLOR_BG)
	match _game_state:
		STATE_MENU:
			_draw_menu()
		STATE_CONSTRUCTION:
			_draw_construction()
		_:
			_draw_play()


func _draw_play() -> void:
	if _shake_time_left > 0.0:
		var fade: float = _shake_time_left / _shake_duration
		var t: float = Time.get_ticks_msec() / 1000.0
		var shake_offset: Vector2 = Vector2(sin(t * 71.0), cos(t * 89.0)) * _shake_strength * fade
		draw_set_transform(shake_offset)
	_draw_panels()
	draw_rect(Rect2(BOARD_ORIGIN, BOARD_SIZE), COLOR_BOARD)
	_draw_tiles(false)
	_draw_powerups()
	_draw_base()
	_draw_spawn_flashes()
	for player: PlayerSlot in _players:
		if player.alive:
			_draw_tank(player.position, player.direction, _player_color(player), false)
			_draw_invulnerability_waves(player)
	for enemy: EnemyTank in _enemies:
		_draw_enemy_tank(enemy)
	for bullet: Bullet in _bullets:
		var trail: Vector2 = bullet.position - Vector2(bullet.direction) * 10.0
		draw_circle(trail, BULLET_RADIUS * 0.7, Color(1.0, 0.85, 0.40, 0.35))
		draw_circle(bullet.position, BULLET_RADIUS, COLOR_BULLET)
	_draw_explosions()
	_draw_particles()
	_draw_tiles(true)
	_draw_score_popups()
	_draw_overlay_message()
	draw_set_transform(Vector2.ZERO)
	if _paused:
		_draw_pause_overlay()
	if _stage_intro_time_left > 0.0:
		_draw_stage_intro()


# ============================================================================
# FLOW
# ============================================================================
func _start_new_campaign() -> void:
	_stage_index = 0
	_players.clear()
	var player_count: int = 2 if _two_players else 1
	for i: int in range(player_count):
		_players.append(PlayerSlot.new(i))
	_start_stage()


func _advance_stage() -> void:
	if _stage_index >= STAGE_COUNT - 1:
		_start_new_campaign()
		return
	_stage_index += 1
	_start_stage()


func _start_stage() -> void:
	_game_state = STATE_PLAYING
	for player: PlayerSlot in _players:
		player.position = _tank_cell_center(player.spawn_cell)
		player.direction = Vector2i.UP
		player.queued_turn = Vector2i.ZERO
		player.shoot_cooldown = 0.0
		player.invulnerable_timer = RESPAWN_INVULNERABLE_TIME
		player.alive = player.lives > 0
		player.ice_dir = Vector2i.ZERO
		player.ice_time_left = 0.0
	_kills = 0
	_kills_by_tier = {"a": 0, "b": 0, "c": 0, "d": 0}
	_spawned_enemies = 0
	_enemy_spawn_index = 0
	_enemy_spawn_timer = ENEMY_FIRST_SPAWN_DELAY
	_base_alive = true
	_base_defence_time_left = 0.0
	_base_defence_fade_timer = 0.0
	_base_defence_steel_active = false
	_freeze_time_left = 0.0
	_game_over = false
	_game_over_progress = 0.0
	_stage_clear = false
	_bonus_screen_time = 0.0
	_paused = false
	_enemies.clear()
	_bullets.clear()
	_spawn_flashes.clear()
	_powerups.clear()
	_explosions.clear()
	_score_popups.clear()
	_particles.clear()
	_shake_time_left = 0.0
	_stage_intro_time_left = STAGE_INTRO_TIME
	_play_sfx("stage_start")
	_load_stage_layout(_stage_index)
	_protect_base_with_brick()
	_clear_spawn_zones()
	_update_status()


func _check_stage_clear() -> void:
	if _stage_clear or _game_over:
		return
	var any_alive: bool = false
	for player: PlayerSlot in _players:
		if player.alive or player.lives > 0:
			any_alive = true
			break
	if not any_alive:
		_game_over = true
		_play_sfx("game_over")
		return
	if _kills >= ENEMIES_PER_STAGE and _enemies.is_empty() and _spawn_flashes.is_empty():
		_stage_clear = true
		_bonus_screen_time = 1.5
		_play_sfx("stage_clear")


# ============================================================================
# STAGE LOADING
# ============================================================================
func _load_stage_layout(index: int) -> void:
	_tiles.clear()
	for sub_y: int in range(DESTRUCTION_GRID_ROWS):
		var sub_row: Array[int] = []
		for sub_x: int in range(DESTRUCTION_GRID_COLUMNS):
			sub_row.append(TILE_EMPTY)
		_tiles.append(sub_row)

	var layout: Array = STAGE_LAYOUTS[index]
	for y: int in range(GRID_ROWS):
		var line: String = String(layout[y])
		for x: int in range(GRID_COLUMNS):
			_set_macro_tile(Vector2i(x, y), _tile_from_char(line.substr(x, 1)))


func _tile_from_char(value: String) -> int:
	match value:
		"B":
			return TILE_BRICK
		"S":
			return TILE_STEEL
		"W":
			return TILE_WATER
		"F":
			return TILE_FOREST
		"I":
			return TILE_ICE
		_:
			return TILE_EMPTY


func _protect_base_with_brick() -> void:
	_set_base_walls(TILE_BRICK)
	for cell: Vector2i in BASE_MACRO_CELLS:
		_set_macro_tile(cell, TILE_EMPTY)


func _set_base_walls(tile: int) -> void:
	# Cattle-bity Base WALL_REGIONS in destruction-cell units (16 px each):
	#   Top:   x 22..29, y 46..47   (4 sub-tiles wide, 1 sub-tile tall)
	#   Left:  x 22..23, y 48..51   (1 sub-tile wide, 2 sub-tiles tall)
	#   Right: x 28..29, y 48..51   (1 sub-tile wide, 2 sub-tiles tall)
	for destruction_y: int in range(46, 48):
		for destruction_x: int in range(22, 30):
			_set_tile(Vector2i(destruction_x, destruction_y), tile)
	for destruction_y: int in range(48, 52):
		for destruction_x: int in range(22, 24):
			_set_tile(Vector2i(destruction_x, destruction_y), tile)
		for destruction_x: int in range(28, 30):
			_set_tile(Vector2i(destruction_x, destruction_y), tile)


func _clear_spawn_zones() -> void:
	for player: PlayerSlot in _players:
		_clear_tank_spawn_area(player.spawn_cell)
	for spawn_cell: Vector2i in SPAWN_CELLS:
		_clear_tank_spawn_area(spawn_cell)
	_protect_base_with_brick()


func _clear_tank_spawn_area(cell: Vector2i) -> void:
	for offset_y: int in range(2):
		for offset_x: int in range(2):
			_set_macro_tile(cell + Vector2i(offset_x, offset_y), TILE_EMPTY)


# ============================================================================
# PLAYER
# ============================================================================
func _update_player(player: PlayerSlot, delta: float) -> void:
	if not player.alive:
		return
	var input_direction: Vector2i = _get_player_input_direction(player)

	if input_direction != Vector2i.ZERO and input_direction != player.direction:
		if input_direction != -player.direction:
			# Perpendicular turn: snap to nearest sub-tile lane only when it's
			# walkable AND close enough (≤ TURN_SNAP_DISTANCE) to feel smooth
			# at corridor intersections. Otherwise rotate in place — that way
			# the player can always face any direction to aim, even from a
			# tight niche, even if the movement in that direction is blocked.
			var snapped: Vector2 = _aligned_turn_position(player.position, input_direction)
			if snapped != player.position and player.position.distance_to(snapped) <= TURN_SNAP_DISTANCE and _can_tank_move_to(snapped, player):
				player.position = snapped
		player.direction = input_direction
	player.queued_turn = Vector2i.ZERO

	var on_ice: bool = _tile_at_world(player.position) == TILE_ICE
	var move_dir: Vector2i = Vector2i.ZERO
	if input_direction != Vector2i.ZERO or player.queued_turn != Vector2i.ZERO:
		move_dir = player.direction
		if on_ice:
			player.ice_dir = player.direction
			player.ice_time_left = ICE_SLIDE_TIME
	elif on_ice and player.ice_time_left > 0.0:
		move_dir = player.ice_dir
		player.ice_time_left = maxf(0.0, player.ice_time_left - delta)
	else:
		player.ice_time_left = 0.0

	if move_dir == Vector2i.ZERO:
		return

	var next_position: Vector2 = player.position + Vector2(move_dir) * PLAYER_MOVE_SPEED * delta
	if _can_tank_move_to(next_position, player):
		player.position = next_position
		_check_powerup_pickup(player)


func _get_player_input_direction(player: PlayerSlot) -> Vector2i:
	if player.index == 0:
		if Input.is_key_pressed(KEY_A):
			return Vector2i.LEFT
		if Input.is_key_pressed(KEY_D):
			return Vector2i.RIGHT
		if Input.is_key_pressed(KEY_W):
			return Vector2i.UP
		if Input.is_key_pressed(KEY_S):
			return Vector2i.DOWN
		if not _two_players:
			if Input.is_key_pressed(KEY_LEFT):
				return Vector2i.LEFT
			if Input.is_key_pressed(KEY_RIGHT):
				return Vector2i.RIGHT
			if Input.is_key_pressed(KEY_UP):
				return Vector2i.UP
			if Input.is_key_pressed(KEY_DOWN):
				return Vector2i.DOWN
	else:
		if Input.is_key_pressed(KEY_LEFT):
			return Vector2i.LEFT
		if Input.is_key_pressed(KEY_RIGHT):
			return Vector2i.RIGHT
		if Input.is_key_pressed(KEY_UP):
			return Vector2i.UP
		if Input.is_key_pressed(KEY_DOWN):
			return Vector2i.DOWN
	return Vector2i.ZERO


func _player_pressed_fire(player: PlayerSlot) -> bool:
	if player.shoot_cooldown > 0.0:
		return false
	if player.index == 0:
		if Input.is_key_pressed(KEY_SPACE):
			return true
		if not _two_players and (Input.is_key_pressed(KEY_ENTER) or Input.is_key_pressed(KEY_KP_ENTER)):
			return true
		return false
	return Input.is_key_pressed(KEY_ENTER) or Input.is_key_pressed(KEY_KP_ENTER)


func _player_shoot(player: PlayerSlot) -> void:
	if player.shoot_cooldown > 0.0:
		return
	var bullet_max: int = MAX_PLAYER_BULLETS_BASE if player.stars < STAR_TWO_BULLETS else MAX_PLAYER_BULLETS_UPGRADED
	if _count_player_bullets(player) >= bullet_max:
		return
	var bullet_speed: float = BULLET_SPEED_SLOW if player.stars < STAR_FAST_BULLET else BULLET_SPEED_FAST
	var high_damage: bool = player.stars >= STAR_BREAK_STEEL
	_spawn_bullet(player.position, player.direction, bullet_speed, "player_%d" % player.index, high_damage)
	_spawn_muzzle_flash(player.position + Vector2(player.direction) * BULLET_SPAWN_OFFSET, Vector2(player.direction))
	_play_sfx("shoot")
	player.shoot_cooldown = PLAYER_SHOOT_COOLDOWN_FAST if player.stars >= STAR_TWO_BULLETS else PLAYER_SHOOT_COOLDOWN_SLOW


func _count_player_bullets(player: PlayerSlot) -> int:
	var count: int = 0
	var party: String = "player_%d" % player.index
	for bullet: Bullet in _bullets:
		if bullet.owner_party == party:
			count += 1
	return count


func _player_hit(player: PlayerSlot) -> void:
	if player.invulnerable_timer > 0.0:
		return
	if player.stars > STAR_NONE:
		player.stars = STAR_NONE
		player.invulnerable_timer = 1.2
		_play_sfx("armor_hit")
		return
	_spawn_explosion(player.position, true)
	_spawn_sparks(player.position, Vector2.ZERO, 14, Color(1.0, 0.65, 0.20, 1.0), 180.0)
	_add_shake(8.0, 0.5)
	_play_sfx("explosion_big")
	player.lives -= 1
	if player.lives <= 0:
		player.alive = false
		return
	player.position = _tank_cell_center(player.spawn_cell)
	player.direction = Vector2i.UP
	player.queued_turn = Vector2i.ZERO
	player.invulnerable_timer = RESPAWN_INVULNERABLE_TIME
	player.ice_time_left = 0.0


func _award_score(player: PlayerSlot, amount: int) -> void:
	player.score += amount
	if not player.extra_life_awarded and player.score >= EXTRA_LIFE_THRESHOLD:
		player.extra_life_awarded = true
		player.lives += 1
		_play_sfx("life")
	if player.score > _high_score:
		_high_score = player.score
		_save_high_score()


func _total_score() -> int:
	var sum: int = 0
	for player: PlayerSlot in _players:
		sum += player.score
	return sum


func _nearest_player_to(target_position: Vector2) -> PlayerSlot:
	var best: PlayerSlot = null
	var best_dist: float = INF
	for player: PlayerSlot in _players:
		if not player.alive:
			continue
		var d: float = player.position.distance_to(target_position)
		if d < best_dist:
			best_dist = d
			best = player
	return best


# ============================================================================
# ENEMIES
# ============================================================================
func _update_enemies(delta: float) -> void:
	for enemy: EnemyTank in _enemies:
		enemy.hit_flash = maxf(0.0, enemy.hit_flash - delta)
	if _freeze_time_left > 0.0:
		return
	for enemy: EnemyTank in _enemies:
		enemy.move_timer -= delta
		enemy.shoot_timer -= delta

		if enemy.move_timer <= 0.0:
			enemy.direction = _pick_enemy_direction(enemy.position)
			enemy.move_timer = randf_range(ENEMY_TURN_DELAY_MIN, ENEMY_TURN_DELAY_MAX)

		var next_position: Vector2 = enemy.position + Vector2(enemy.direction) * enemy.move_speed * delta
		if _can_enemy_move_to(next_position, enemy):
			enemy.position = next_position
		else:
			enemy.direction = _pick_enemy_direction(enemy.position)

		if enemy.shoot_timer <= 0.0:
			_spawn_bullet(enemy.position, enemy.direction, enemy.bullet_speed, "enemy", false)
			_spawn_muzzle_flash(enemy.position + Vector2(enemy.direction) * BULLET_SPAWN_OFFSET, Vector2(enemy.direction))
			enemy.shoot_timer = randf_range(ENEMY_SHOOT_DELAY_MIN, ENEMY_SHOOT_DELAY_MAX)


func _pick_enemy_direction(enemy_position: Vector2) -> Vector2i:
	var directions: Array[Vector2i] = [Vector2i.UP, Vector2i.RIGHT, Vector2i.DOWN, Vector2i.LEFT]
	if randf() < 0.46:
		var to_base: Vector2 = _tank_cell_center(Vector2i(12, 24)) - enemy_position
		if absf(to_base.x) > absf(to_base.y):
			return Vector2i.RIGHT if to_base.x > 0.0 else Vector2i.LEFT
		return Vector2i.DOWN if to_base.y > 0.0 else Vector2i.UP
	return directions[randi_range(0, directions.size() - 1)]


func _spawn_enemies_if_needed(delta: float) -> void:
	if _spawned_enemies + _spawn_flashes.size() >= ENEMIES_PER_STAGE:
		return
	if _enemies.size() + _spawn_flashes.size() >= MAX_ENEMIES_ON_FIELD:
		return
	_enemy_spawn_timer = maxf(0.0, _enemy_spawn_timer - delta)
	if _enemy_spawn_timer > 0.0:
		return
	var spawn_cell: Vector2i = SPAWN_CELLS[_enemy_spawn_index % SPAWN_CELLS.size()]
	var spawn_position: Vector2 = _tank_cell_center(spawn_cell)
	if _is_position_blocked_by_tank(spawn_position):
		_enemy_spawn_timer = 0.4
		return
	var tier: String = STAGE_ENEMY_TIERS[_stage_index][_spawned_enemies]
	var drop: bool = STAGE_ENEMY_DROP_INDICES[_stage_index].has(_spawned_enemies)
	_spawn_flashes.append(SpawnFlash.new(spawn_position, tier, drop))
	_spawned_enemies += 1
	_enemy_spawn_index += 1
	_enemy_spawn_timer = ENEMY_SPAWN_DELAY


func _update_spawn_flashes(delta: float) -> void:
	for index: int in range(_spawn_flashes.size() - 1, -1, -1):
		var flash: SpawnFlash = _spawn_flashes[index]
		flash.time_left -= delta
		if flash.time_left <= 0.0:
			_enemies.append(EnemyTank.new(flash.position, flash.pending_tier, flash.pending_drop))
			_spawn_flashes.remove_at(index)


# ============================================================================
# BULLETS
# ============================================================================
func _spawn_bullet(tank_position: Vector2, direction: Vector2i, speed: float, party: String, high_damage: bool) -> void:
	var spawn_position: Vector2 = tank_position + Vector2(direction) * BULLET_SPAWN_OFFSET
	_bullets.append(Bullet.new(spawn_position, direction, speed, party, high_damage))


func _update_bullets(delta: float) -> void:
	for index: int in range(_bullets.size() - 1, -1, -1):
		var bullet: Bullet = _bullets[index]
		if _advance_bullet(bullet, delta):
			_bullets.remove_at(index)


func _advance_bullet(bullet: Bullet, delta: float) -> bool:
	var distance: float = bullet.speed * delta
	var steps: int = max(1, int(ceilf(distance / BULLET_TRACE_STEP)))
	var step_distance: float = distance / float(steps)
	for step_index: int in range(steps):
		bullet.position += Vector2(bullet.direction) * step_distance
		if _bullet_hits_other_bullet(bullet):
			return true
		if _handle_bullet_collision(bullet):
			return true
	return false


func _bullet_hits_other_bullet(bullet: Bullet) -> bool:
	var bullet_is_player: bool = bullet.owner_party.begins_with("player")
	for other_index: int in range(_bullets.size() - 1, -1, -1):
		var other: Bullet = _bullets[other_index]
		if other == bullet:
			continue
		var other_is_player: bool = other.owner_party.begins_with("player")
		if bullet_is_player == other_is_player:
			continue
		if bullet.position.distance_to(other.position) <= BULLET_RADIUS * 2.0 + 2.0:
			_spawn_sparks(bullet.position, Vector2.ZERO, 4, Color(1.0, 0.95, 0.70, 1.0), 120.0)
			_bullets.remove_at(other_index)
			return true
	return false


func _handle_bullet_collision(bullet: Bullet) -> bool:
	if not _point_inside_board(bullet.position):
		return true

	for bullet_cell: Vector2i in _bullet_contact_cells(bullet.position, bullet.direction):
		var tile: int = _tile_at_cell(bullet_cell)
		if tile == TILE_STEEL:
			if bullet.wall_damage_high:
				_destroy_steel_macro(bullet_cell)
				_spawn_sparks(bullet.position, Vector2(bullet.direction), 8, Color(0.85, 0.88, 0.90, 1.0), 160.0)
				_play_sfx("brick")
				return true
			_spawn_sparks(bullet.position, Vector2(bullet.direction), 5, Color(1.0, 1.0, 0.80, 1.0), 190.0)
			_play_sfx("steel", -6.0)
			return true

		if tile == TILE_BRICK:
			var depth: int = 2 if bullet.wall_damage_high else 1
			_destroy_brick_slice(bullet_cell, bullet.direction, depth, bullet.position)
			_spawn_sparks(bullet.position, Vector2(bullet.direction), 7, COLOR_BRICK.lightened(0.15), 150.0)
			_play_sfx("brick")
			return true

		if _is_base_subcell(bullet_cell):
			_base_alive = false
			_game_over = true
			_spawn_explosion(_tank_cell_center(Vector2i(12, 24)), true)
			_spawn_sparks(_tank_cell_center(Vector2i(12, 24)), Vector2.ZERO, 22, Color(1.0, 0.70, 0.20, 1.0), 220.0)
			_add_shake(14.0, 0.9)
			_play_sfx("explosion_big")
			_play_sfx("game_over")
			return true

	if bullet.owner_party.begins_with("player"):
		return _try_hit_enemy(bullet)

	for player: PlayerSlot in _players:
		if player.alive and bullet.position.distance_to(player.position) <= TANK_RADIUS + BULLET_RADIUS:
			_player_hit(player)
			return true
	return false


func _bullet_contact_cells(position: Vector2, direction: Vector2i) -> Array[Vector2i]:
	var cells: Array[Vector2i] = []
	var offsets: Array[Vector2] = []
	if direction.x != 0:
		offsets = [
			Vector2(0.0, -DESTRUCTION_CELL_SIZE * 1.5),
			Vector2(0.0, -DESTRUCTION_CELL_SIZE * 0.5),
			Vector2(0.0, DESTRUCTION_CELL_SIZE * 0.5),
			Vector2(0.0, DESTRUCTION_CELL_SIZE * 1.5),
		]
	else:
		offsets = [
			Vector2(-DESTRUCTION_CELL_SIZE * 1.5, 0.0),
			Vector2(-DESTRUCTION_CELL_SIZE * 0.5, 0.0),
			Vector2(DESTRUCTION_CELL_SIZE * 0.5, 0.0),
			Vector2(DESTRUCTION_CELL_SIZE * 1.5, 0.0),
		]
	for offset: Vector2 in offsets:
		var cell: Vector2i = _world_to_destruction_cell(position + offset)
		if not cells.has(cell):
			cells.append(cell)
	return cells


func _try_hit_enemy(bullet: Bullet) -> bool:
	for index: int in range(_enemies.size() - 1, -1, -1):
		var enemy: EnemyTank = _enemies[index]
		if bullet.position.distance_to(enemy.position) <= TANK_RADIUS + DESTRUCTION_CELL_SIZE:
			enemy.hp -= 1
			if enemy.hp <= 0:
				_kill_enemy(index, _player_for_party(bullet.owner_party))
			else:
				enemy.hit_flash = 0.15
				_spawn_sparks(bullet.position, Vector2(bullet.direction), 5, Color(1.0, 1.0, 0.85, 1.0), 160.0)
				_play_sfx("armor_hit")
			return true
	return false


func _player_for_party(party: String) -> PlayerSlot:
	if party == "player_0" and _players.size() > 0:
		return _players[0]
	if party == "player_1" and _players.size() > 1:
		return _players[1]
	if _players.size() > 0:
		return _players[0]
	return null


func _kill_enemy(index: int, scoring_player: PlayerSlot) -> void:
	var enemy: EnemyTank = _enemies[index]
	var enemy_position: Vector2 = enemy.position
	var tier: String = enemy.tier
	var was_drop: bool = enemy.is_drop_carrier
	_enemies.remove_at(index)
	_kills += 1
	_kills_by_tier[tier] = int(_kills_by_tier[tier]) + 1
	var points: int = int(SCORE_BY_TIER[tier])
	if scoring_player != null:
		_award_score(scoring_player, points)
	_score_popups.append(ScorePopup.new(enemy_position, str(points)))
	_spawn_explosion(enemy_position, true)
	_spawn_sparks(enemy_position, Vector2.ZERO, 10, Color(1.0, 0.62, 0.18, 1.0), 200.0)
	_add_shake(4.0, 0.25)
	_play_sfx("explosion")
	if was_drop:
		_spawn_random_powerup()


func _destroy_brick_slice(hit_cell: Vector2i, direction: Vector2i, depth_cells: int, bullet_position: Vector2 = Vector2(-1000000.0, -1000000.0)) -> void:
	# Match cattle-bity TerrainTileDestroyer: tank-wide span perpendicular to
	# bullet (so the hole always matches the tank footprint and the tank can
	# pass through it), `depth_cells` deep along bullet direction.
	# Low damage = 1, High = 2.
	if direction.x != 0:
		var column_start: int
		if direction.x > 0:
			column_start = hit_cell.x
		else:
			column_start = hit_cell.x - depth_cells + 1
		var row_start: int
		var row_end: int
		if bullet_position.y < -999000.0:
			row_start = hit_cell.y - DESTRUCTION_SUBDIVISIONS
			row_end = hit_cell.y + DESTRUCTION_SUBDIVISIONS - 1
		else:
			row_start = int(floorf((bullet_position.y - TANK_RADIUS - BOARD_ORIGIN.y) / float(DESTRUCTION_CELL_SIZE)))
			row_end = int(floorf((bullet_position.y + TANK_RADIUS - BOARD_ORIGIN.y) / float(DESTRUCTION_CELL_SIZE)))
		for offset_x: int in range(depth_cells):
			for row: int in range(row_start, row_end + 1):
				_set_tile(Vector2i(column_start + offset_x, row), TILE_EMPTY)
	else:
		var row_start: int
		if direction.y > 0:
			row_start = hit_cell.y
		else:
			row_start = hit_cell.y - depth_cells + 1
		var col_start: int
		var col_end: int
		if bullet_position.x < -999000.0:
			col_start = hit_cell.x - DESTRUCTION_SUBDIVISIONS
			col_end = hit_cell.x + DESTRUCTION_SUBDIVISIONS - 1
		else:
			col_start = int(floorf((bullet_position.x - TANK_RADIUS - BOARD_ORIGIN.x) / float(DESTRUCTION_CELL_SIZE)))
			col_end = int(floorf((bullet_position.x + TANK_RADIUS - BOARD_ORIGIN.x) / float(DESTRUCTION_CELL_SIZE)))
		for offset_y: int in range(depth_cells):
			for col: int in range(col_start, col_end + 1):
				_set_tile(Vector2i(col, row_start + offset_y), TILE_EMPTY)


func _destroy_steel_macro(hit_cell: Vector2i) -> void:
	var macro_top_left: Vector2i = Vector2i(int(hit_cell.x / DESTRUCTION_SUBDIVISIONS) * DESTRUCTION_SUBDIVISIONS, int(hit_cell.y / DESTRUCTION_SUBDIVISIONS) * DESTRUCTION_SUBDIVISIONS)
	for offset_y: int in range(DESTRUCTION_SUBDIVISIONS):
		for offset_x: int in range(DESTRUCTION_SUBDIVISIONS):
			_set_tile(macro_top_left + Vector2i(offset_x, offset_y), TILE_EMPTY)


func _destruction_axis_index(world_value: float, origin_value: float, fallback: int) -> int:
	if world_value < -999000.0:
		return fallback
	return int(roundf((world_value - origin_value) / DESTRUCTION_CELL_SIZE))


# ============================================================================
# POWERUPS
# ============================================================================
func _spawn_random_powerup() -> void:
	var powerup_type: String = POWERUP_TYPES[randi_range(0, POWERUP_TYPES.size() - 1)]
	var random_cell: Vector2i = Vector2i(randi_range(2, GRID_COLUMNS - 4), randi_range(2, GRID_ROWS - 4))
	var spawn_position: Vector2 = _tank_cell_center(random_cell)
	_powerups.clear()
	_powerups.append(PowerupItem.new(spawn_position, powerup_type))
	_play_sfx("powerup_spawn")


func _update_powerups(delta: float) -> void:
	for index: int in range(_powerups.size() - 1, -1, -1):
		var powerup: PowerupItem = _powerups[index]
		powerup.lifetime -= delta
		powerup.blink_timer += delta
		if powerup.lifetime <= 0.0:
			_powerups.remove_at(index)


func _check_powerup_pickup(player: PlayerSlot) -> void:
	for index: int in range(_powerups.size() - 1, -1, -1):
		var powerup: PowerupItem = _powerups[index]
		if player.position.distance_to(powerup.position) <= TANK_RADIUS + 22.0:
			_apply_powerup(player, powerup.type)
			_play_sfx("pickup")
			_powerups.remove_at(index)
			_award_score(player, POWERUP_PICKUP_POINTS)
			_score_popups.append(ScorePopup.new(powerup.position, str(POWERUP_PICKUP_POINTS)))


func _apply_powerup(player: PlayerSlot, type: String) -> void:
	match type:
		"shield":
			player.invulnerable_timer = SHIELD_DURATION
		"upgrade":
			player.stars = mini(STAR_BREAK_STEEL, player.stars + 1)
		"wipeout":
			for enemy: EnemyTank in _enemies:
				_spawn_explosion(enemy.position, true)
				_spawn_sparks(enemy.position, Vector2.ZERO, 10, Color(1.0, 0.62, 0.18, 1.0), 200.0)
				_kills += 1
				_kills_by_tier[enemy.tier] = int(_kills_by_tier[enemy.tier]) + 1
			_enemies.clear()
			_add_shake(10.0, 0.6)
			_play_sfx("explosion_big")
		"life":
			player.lives += 1
		"defence":
			_activate_base_defence()
		"freeze":
			_freeze_time_left = FREEZE_DURATION
			_play_sfx("freeze")


func _activate_base_defence() -> void:
	_base_defence_time_left = BASE_DEFENCE_DURATION
	_base_defence_fade_timer = 0.0
	_base_defence_steel_active = true
	_set_base_walls(TILE_STEEL)


func _update_base_defence(delta: float) -> void:
	if _base_defence_time_left <= 0.0:
		return
	_base_defence_time_left -= delta
	if _base_defence_time_left <= 0.0:
		_base_defence_time_left = 0.0
		_base_defence_steel_active = false
		_set_base_walls(TILE_BRICK)
		return
	if _base_defence_time_left <= BASE_DEFENCE_FADE_TIME:
		_base_defence_fade_timer -= delta
		if _base_defence_fade_timer <= 0.0:
			_base_defence_fade_timer = BASE_DEFENCE_FADE_INTERVAL
			_base_defence_steel_active = not _base_defence_steel_active
			_set_base_walls(TILE_STEEL if _base_defence_steel_active else TILE_BRICK)


# ============================================================================
# EXPLOSIONS / FX
# ============================================================================
func _spawn_explosion(position: Vector2, big: bool) -> void:
	_explosions.append(Explosion.new(position, big))


func _update_explosions(delta: float) -> void:
	for index: int in range(_explosions.size() - 1, -1, -1):
		var explosion: Explosion = _explosions[index]
		explosion.time_left -= delta
		if explosion.time_left <= 0.0:
			_explosions.remove_at(index)


func _update_score_popups(delta: float) -> void:
	for index: int in range(_score_popups.size() - 1, -1, -1):
		var popup: ScorePopup = _score_popups[index]
		popup.time_left -= delta
		popup.position += Vector2(0.0, -18.0 * delta)
		if popup.time_left <= 0.0:
			_score_popups.remove_at(index)


# ============================================================================
# COLLISIONS
# ============================================================================
func _can_turn_to(player: PlayerSlot, direction: Vector2i) -> bool:
	if direction == player.direction:
		return true
	if direction == -player.direction:
		return true
	# Perpendicular: snap to the nearest sub-tile lane and allow the turn only
	# if that snapped position is itself walkable. This is the simple
	# Battle-City-style behavior — if the tank is wedged in a niche and the
	# nearest lane is blocked by leftover bricks, the player has to back out
	# a few pixels before turning sideways.
	var snapped: Vector2 = _aligned_turn_position(player.position, direction)
	return _can_tank_move_to(snapped, player)


func _aligned_turn_position(position: Vector2, direction: Vector2i) -> Vector2:
	var aligned_position: Vector2 = position
	if direction.x != 0:
		aligned_position.y = _nearest_lane_center(position.y, BOARD_ORIGIN.y)
	else:
		aligned_position.x = _nearest_lane_center(position.x, BOARD_ORIGIN.x)
	return aligned_position


func _nearest_lane_center(world_value: float, origin_value: float) -> float:
	# Tank center is on the sub-tile grid (cattle-bity moves on 32-px units).
	# Tank radius is TANK_DRAW_SIZE/2; tank center must be inside [origin + half, origin + size - half].
	var local_value: float = world_value - origin_value
	var lane_index: int = int(roundf(local_value / float(MOVEMENT_CELL_SIZE)))
	var min_lane: int = int(ceilf(TANK_DRAW_SIZE * 0.5 / float(MOVEMENT_CELL_SIZE)))
	var board_extent: int = GRID_COLUMNS if origin_value == BOARD_ORIGIN.x else GRID_ROWS
	var max_lane: int = board_extent - min_lane
	lane_index = clampi(lane_index, min_lane, max_lane)
	return origin_value + float(lane_index * MOVEMENT_CELL_SIZE)


func _can_tank_move_to(position: Vector2, self_player: PlayerSlot) -> bool:
	if not _tank_inside_board(position):
		return false
	var occupied_cells: Array[Vector2i] = _tank_occupied_cells(position)
	for cell: Vector2i in occupied_cells:
		var tile: int = _tile_at_cell(cell)
		if tile == TILE_BRICK or tile == TILE_STEEL or tile == TILE_WATER:
			return false
		if _is_base_subcell(cell):
			return false
	for enemy: EnemyTank in _enemies:
		if position.distance_to(enemy.position) < TANK_RADIUS * 2.0:
			return false
	for other: PlayerSlot in _players:
		if other == self_player or not other.alive:
			continue
		if position.distance_to(other.position) < TANK_RADIUS * 2.0:
			return false
	return true


func _can_enemy_move_to(position: Vector2, self_enemy: EnemyTank) -> bool:
	if not _tank_inside_board(position):
		return false
	var occupied_cells: Array[Vector2i] = _tank_occupied_cells(position)
	for cell: Vector2i in occupied_cells:
		var tile: int = _tile_at_cell(cell)
		if tile == TILE_BRICK or tile == TILE_STEEL or tile == TILE_WATER:
			return false
		if _is_base_subcell(cell):
			return false
	for other: EnemyTank in _enemies:
		if other == self_enemy:
			continue
		if position.distance_to(other.position) < TANK_RADIUS * 2.0:
			return false
	for player: PlayerSlot in _players:
		if player.alive and position.distance_to(player.position) < TANK_RADIUS * 2.0:
			return false
	return true


func _tank_inside_board(position: Vector2) -> bool:
	if position.x - TANK_RADIUS < BOARD_ORIGIN.x or position.y - TANK_RADIUS < BOARD_ORIGIN.y:
		return false
	if position.x + TANK_RADIUS > BOARD_ORIGIN.x + BOARD_SIZE.x:
		return false
	if position.y + TANK_RADIUS > BOARD_ORIGIN.y + BOARD_SIZE.y:
		return false
	return true


func _tank_occupied_cells(position: Vector2) -> Array[Vector2i]:
	# Check every destruction cell that overlaps the tank's bounding box, not
	# just the four corners. With smooth (non-grid-locked) tank movement the
	# tank can straddle 5 sub-tile rows perpendicular to its travel direction,
	# and corner-only sampling lets internal brick fragments slip through.
	var min_cell: Vector2i = _world_to_destruction_cell(position - Vector2(TANK_RADIUS, TANK_RADIUS))
	var max_cell: Vector2i = _world_to_destruction_cell(position + Vector2(TANK_RADIUS, TANK_RADIUS))
	var cells: Array[Vector2i] = []
	for y: int in range(min_cell.y, max_cell.y + 1):
		for x: int in range(min_cell.x, max_cell.x + 1):
			cells.append(Vector2i(x, y))
	return cells


func _is_position_blocked_by_tank(position: Vector2) -> bool:
	for player: PlayerSlot in _players:
		if player.alive and position.distance_to(player.position) < TANK_RADIUS * 2.0:
			return true
	for enemy: EnemyTank in _enemies:
		if position.distance_to(enemy.position) < TANK_RADIUS * 2.0:
			return true
	return false


# ============================================================================
# DRAWING
# ============================================================================
func _draw_panels() -> void:
	# Full gray frame around the field on all 4 sides.
	draw_rect(Rect2(Vector2.ZERO, SCREEN_SIZE), COLOR_PANEL)

	var lx: float = 24.0
	draw_string(ThemeDB.fallback_font, Vector2(lx, 70.0), "TANKS", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 38, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(lx, 102.0), "BATTLE CITY", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 20, Color.BLACK)
	if _two_players:
		draw_string(ThemeDB.fallback_font, Vector2(lx, 132.0), "P1 WASD + Space", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)
		draw_string(ThemeDB.fallback_font, Vector2(lx, 150.0), "P2 arrows + Enter", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)
	else:
		draw_string(ThemeDB.fallback_font, Vector2(lx, 132.0), "WASD / arrows", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)
		draw_string(ThemeDB.fallback_font, Vector2(lx, 150.0), "Space / Enter fire", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(lx, 168.0), "Esc menu / pause", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)

	draw_string(ThemeDB.fallback_font, Vector2(lx, 210.0), "HI-SCORE", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 16, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(lx, 236.0), str(_high_score), HORIZONTAL_ALIGNMENT_LEFT, -1.0, 22, Color.BLACK)

	var y: float = 280.0
	for player: PlayerSlot in _players:
		var label: String = "P%d" % (player.index + 1)
		draw_string(ThemeDB.fallback_font, Vector2(lx, y), label, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 18, Color.BLACK)
		draw_rect(Rect2(Vector2(lx + 30.0, y - 14.0), Vector2(14.0, 14.0)), player.color)
		draw_string(ThemeDB.fallback_font, Vector2(lx, y + 22.0), "score %d" % player.score, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 14, Color.BLACK)
		draw_string(ThemeDB.fallback_font, Vector2(lx, y + 40.0), "lives %d" % player.lives, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 14, Color.BLACK)
		for star_i: int in range(player.stars):
			draw_circle(Vector2(lx + 100.0 + float(star_i) * 16.0, y + 35.0), 5.0, Color(1.0, 0.85, 0.20, 1.0))
		y += 70.0

	var right_x: float = PANEL_RIGHT_X + 24.0
	draw_string(ThemeDB.fallback_font, Vector2(right_x, 78.0), "STAGE", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 22, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(right_x + 16.0, 116.0), str(_stage_index + 1), HORIZONTAL_ALIGNMENT_LEFT, -1.0, 34, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(right_x, 174.0), "ENEMY", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 18, Color.BLACK)
	_draw_enemy_remaining_grid(right_x, 196.0)
	if _freeze_time_left > 0.0:
		draw_string(ThemeDB.fallback_font, Vector2(right_x, 530.0), "FREEZE %.1f" % _freeze_time_left, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 16, Color(0.20, 0.40, 0.85, 1.0))
	if _base_defence_time_left > 0.0:
		draw_string(ThemeDB.fallback_font, Vector2(right_x, 558.0), "DEFENCE %.1f" % _base_defence_time_left, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 16, Color(0.55, 0.55, 0.55, 1.0))


func _draw_enemy_remaining_grid(origin_x: float, origin_y: float) -> void:
	var remaining: int = ENEMIES_PER_STAGE - _spawned_enemies + _spawn_flashes.size() + _enemies.size()
	if remaining < 0:
		remaining = 0
	var cell_size: float = 26.0
	for i: int in range(ENEMIES_PER_STAGE):
		var col: int = i % 2
		var row: int = i / 2
		var pos: Vector2 = Vector2(origin_x + float(col) * cell_size, origin_y + float(row) * cell_size)
		var rect: Rect2 = Rect2(pos, Vector2(cell_size - 4.0, cell_size - 4.0))
		if i < remaining:
			_draw_mini_enemy_icon(rect)
		else:
			draw_rect(rect, Color(0.32, 0.32, 0.32, 0.4))


func _draw_mini_enemy_icon(rect: Rect2) -> void:
	draw_rect(rect, Color(0.18, 0.18, 0.18, 1.0))
	var inner: Rect2 = rect.grow(-4.0)
	draw_rect(inner, Color(0.78, 0.78, 0.78, 1.0))
	var center: Vector2 = rect.position + rect.size * 0.5
	draw_line(center + Vector2(0.0, 1.0), center + Vector2(0.0, -10.0), Color(0.18, 0.18, 0.18, 1.0), 3.0)


func _draw_tiles(forest_only: bool) -> void:
	for y: int in range(GRID_ROWS):
		for x: int in range(GRID_COLUMNS):
			_draw_macro_tile(Vector2i(x, y), forest_only)


func _draw_macro_tile(cell: Vector2i, forest_only: bool) -> void:
	var top_left: Vector2i = cell * DESTRUCTION_SUBDIVISIONS
	var tile: int = _first_visible_tile_in_macro(top_left, forest_only)
	if tile == TILE_EMPTY:
		return
	if forest_only and tile != TILE_FOREST:
		return
	if not forest_only and tile == TILE_FOREST:
		return

	match tile:
		TILE_BRICK:
			_draw_brick_macro(top_left)
		TILE_STEEL:
			_draw_solid_macro(cell, COLOR_STEEL, Color(0.36, 0.38, 0.38, 1.0))
		TILE_WATER:
			_draw_water_macro(cell)
		TILE_FOREST:
			_draw_forest_macro(cell)
		TILE_ICE:
			_draw_solid_macro(cell, COLOR_ICE, Color(0.88, 1.0, 1.0, 0.75))


func _first_visible_tile_in_macro(top_left: Vector2i, forest_only: bool) -> int:
	for offset_y: int in range(DESTRUCTION_SUBDIVISIONS):
		for offset_x: int in range(DESTRUCTION_SUBDIVISIONS):
			var tile: int = _tile_at_cell(top_left + Vector2i(offset_x, offset_y))
			if tile == TILE_EMPTY:
				continue
			if forest_only and tile != TILE_FOREST:
				continue
			if not forest_only and tile == TILE_FOREST:
				continue
			return tile
	return TILE_EMPTY


func _draw_brick_macro(top_left: Vector2i) -> void:
	for offset_y: int in range(DESTRUCTION_SUBDIVISIONS):
		for offset_x: int in range(DESTRUCTION_SUBDIVISIONS):
			var cell: Vector2i = top_left + Vector2i(offset_x, offset_y)
			if _tile_at_cell(cell) != TILE_BRICK:
				continue
			_draw_brick_chunk(_destruction_cell_top_left(cell), cell)


func _draw_brick_chunk(top_left: Vector2, cell: Vector2i) -> void:
	# Each destruction cell holds two horizontal bricks. Vertical mortar offset alternates
	# per row to give the classic NES Battle City brick-laying pattern.
	var w: float = float(DESTRUCTION_CELL_SIZE)
	var h: float = float(DESTRUCTION_CELL_SIZE)
	var rect: Rect2 = Rect2(top_left, Vector2(w, h))
	var mortar: Color = Color(0.30, 0.13, 0.07, 1.0)
	draw_rect(rect, mortar)
	var brick_h: float = h * 0.5 - 1.0
	var half_w: float = w * 0.5 - 1.0
	if cell.y % 2 == 0:
		# even row: top has split, bottom is full
		draw_rect(Rect2(top_left + Vector2(0.0, 1.0), Vector2(half_w, brick_h)), COLOR_BRICK)
		draw_rect(Rect2(top_left + Vector2(half_w + 1.0, 1.0), Vector2(half_w, brick_h)), COLOR_BRICK)
		draw_rect(Rect2(top_left + Vector2(0.0, h * 0.5 + 1.0), Vector2(w, brick_h)), COLOR_BRICK)
	else:
		# odd row: top is full, bottom has split
		draw_rect(Rect2(top_left + Vector2(0.0, 1.0), Vector2(w, brick_h)), COLOR_BRICK)
		draw_rect(Rect2(top_left + Vector2(0.0, h * 0.5 + 1.0), Vector2(half_w, brick_h)), COLOR_BRICK)
		draw_rect(Rect2(top_left + Vector2(half_w + 1.0, h * 0.5 + 1.0), Vector2(half_w, brick_h)), COLOR_BRICK)


func _draw_solid_macro(cell: Vector2i, fill_color: Color, detail_color: Color) -> void:
	var rect: Rect2 = Rect2(_cell_top_left(cell), Vector2(CELL_SIZE, CELL_SIZE))
	draw_rect(rect.grow(-1.0), fill_color)
	draw_rect(rect.grow(-3.0), detail_color, false, 1.0)


func _draw_forest_macro(cell: Vector2i) -> void:
	var rect: Rect2 = Rect2(_cell_top_left(cell), Vector2(CELL_SIZE, CELL_SIZE))
	draw_rect(rect.grow(-1.0), COLOR_FOREST)
	draw_circle(rect.position + Vector2(8.0, 8.0), 5.0, Color(0.04, 0.24, 0.07, 0.85))
	draw_circle(rect.position + Vector2(16.0, 15.0), 6.0, Color(0.05, 0.30, 0.08, 0.85))


func _draw_base() -> void:
	if not _base_alive:
		_draw_base_rubble()
		return
	var left: Vector2 = _cell_top_left(Vector2i(12, 24))
	var size: float = float(CELL_SIZE) * 2.0
	var rect: Rect2 = Rect2(left + Vector2(2.0, 2.0), Vector2(size - 4.0, size - 4.0))
	draw_rect(rect, COLOR_BASE)
	# Stylised eagle silhouette (procedural, no sprite yet).
	var dark: Color = Color(0.20, 0.13, 0.05, 1.0)
	var c: Vector2 = left + Vector2(size * 0.5, size * 0.5)
	# Body diamond
	draw_polygon(
		PackedVector2Array([
			c + Vector2(0.0, -size * 0.34),
			c + Vector2(size * 0.18, -size * 0.05),
			c + Vector2(size * 0.10, size * 0.30),
			c + Vector2(-size * 0.10, size * 0.30),
			c + Vector2(-size * 0.18, -size * 0.05),
		]),
		PackedColorArray([dark])
	)
	# Wings
	draw_polygon(
		PackedVector2Array([
			c + Vector2(-size * 0.18, -size * 0.05),
			c + Vector2(-size * 0.40, size * 0.05),
			c + Vector2(-size * 0.30, size * 0.20),
			c + Vector2(-size * 0.10, size * 0.05),
		]),
		PackedColorArray([dark])
	)
	draw_polygon(
		PackedVector2Array([
			c + Vector2(size * 0.18, -size * 0.05),
			c + Vector2(size * 0.40, size * 0.05),
			c + Vector2(size * 0.30, size * 0.20),
			c + Vector2(size * 0.10, size * 0.05),
		]),
		PackedColorArray([dark])
	)
	# Beak
	draw_polygon(
		PackedVector2Array([
			c + Vector2(0.0, -size * 0.34),
			c + Vector2(size * 0.06, -size * 0.40),
			c + Vector2(-size * 0.06, -size * 0.40),
		]),
		PackedColorArray([dark])
	)


func _draw_tank(position: Vector2, direction: Vector2i, color: Color, draw_armor_band: bool = false) -> void:
	var half_size: float = TANK_DRAW_SIZE * 0.5
	var tread_axis: Vector2 = Vector2(direction.y, direction.x)
	var forward: Vector2 = Vector2(direction)
	var tread_offset: float = TANK_DRAW_SIZE * 0.375

	var phase: int = int(Time.get_ticks_msec() / 90.0) % 2
	_draw_tread(position - tread_axis * tread_offset, forward, phase)
	_draw_tread(position + tread_axis * tread_offset, forward, 1 - phase)

	var body_rect: Rect2 = Rect2(position - Vector2(half_size, half_size), Vector2(TANK_DRAW_SIZE, TANK_DRAW_SIZE)).grow(-4.0)
	draw_rect(body_rect, color)
	draw_rect(body_rect, Color(0.0, 0.0, 0.0, 0.55), false, 2.0)

	if draw_armor_band:
		draw_rect(body_rect.grow(-4.0), Color(1.0, 1.0, 1.0, 0.30), false, 2.0)

	var turret_pivot_radius: float = TANK_DRAW_SIZE * 0.22
	draw_circle(position, turret_pivot_radius, color.darkened(0.35))
	draw_circle(position, turret_pivot_radius - 3.0, color.lightened(0.10))

	var turret_end: Vector2 = position + forward * (TANK_DRAW_SIZE * 0.55)
	draw_line(position, turret_end, Color(0.05, 0.05, 0.05, 1.0), 12.0)
	draw_line(position, turret_end, color.lightened(0.25), 5.0)


func _draw_tread(center: Vector2, forward: Vector2, phase: int) -> void:
	var perp: Vector2 = Vector2(-forward.y, forward.x)
	var length: float = TANK_DRAW_SIZE * 0.5
	var width: float = 9.0
	var corner_a: Vector2 = center - forward * length + perp * width
	var corner_b: Vector2 = center + forward * length + perp * width
	var corner_c: Vector2 = center + forward * length - perp * width
	var corner_d: Vector2 = center - forward * length - perp * width
	draw_polygon(PackedVector2Array([corner_a, corner_b, corner_c, corner_d]), PackedColorArray([Color(0.05, 0.05, 0.05, 1.0)]))
	for chunk: int in range(4):
		if (chunk + phase) % 2 == 0:
			continue
		var chunk_center: Vector2 = center + forward * (float(chunk) * (length * 0.5) - length + length * 0.25)
		var chunk_a: Vector2 = chunk_center - forward * (length * 0.18) + perp * (width - 1.5)
		var chunk_b: Vector2 = chunk_center + forward * (length * 0.18) + perp * (width - 1.5)
		var chunk_c: Vector2 = chunk_center + forward * (length * 0.18) - perp * (width - 1.5)
		var chunk_d: Vector2 = chunk_center - forward * (length * 0.18) - perp * (width - 1.5)
		draw_polygon(PackedVector2Array([chunk_a, chunk_b, chunk_c, chunk_d]), PackedColorArray([Color(0.40, 0.38, 0.36, 1.0)]))


func _draw_enemy_tank(enemy: EnemyTank) -> void:
	var color: Color = _enemy_color(enemy)
	var draw_band: bool = enemy.tier == "d"
	if enemy.is_drop_carrier:
		var phase: float = fmod(Time.get_ticks_msec() / 180.0, 2.0)
		if phase < 1.0:
			color = COLOR_DROP_FLASH
	if _freeze_time_left > 0.0:
		var thaw_blink: bool = _freeze_time_left < 1.5 and fmod(Time.get_ticks_msec() / 250.0, 2.0) < 1.0
		if not thaw_blink:
			color = color.lerp(Color(0.55, 0.80, 1.0, 1.0), 0.5)
	if enemy.hit_flash > 0.0:
		color = Color(1.0, 1.0, 1.0, 1.0)
	_draw_tank(enemy.position, enemy.direction, color, draw_band)


func _enemy_color(enemy: EnemyTank) -> Color:
	match enemy.tier:
		"a":
			return COLOR_ENEMY_BASIC
		"b":
			return COLOR_ENEMY_FAST
		"c":
			return COLOR_ENEMY_POWER
		"d":
			match enemy.hp:
				4:
					return COLOR_ENEMY_ARMOR_FULL
				3:
					return COLOR_ENEMY_ARMOR_3
				2:
					return COLOR_ENEMY_ARMOR_2
				_:
					return COLOR_ENEMY_ARMOR_1
		_:
			return COLOR_ENEMY_BASIC


func _player_color(player: PlayerSlot) -> Color:
	var base: Color = player.color
	if player.stars >= STAR_BREAK_STEEL:
		return base.lightened(0.30)
	if player.stars >= STAR_TWO_BULLETS:
		return base.lightened(0.18)
	if player.stars >= STAR_FAST_BULLET:
		return base.lightened(0.08)
	return base


func _draw_invulnerability_waves(player: PlayerSlot) -> void:
	if player.invulnerable_timer <= 0.0 or _game_over:
		return
	var progress: float = 1.0 - (player.invulnerable_timer / max(RESPAWN_INVULNERABLE_TIME, SHIELD_DURATION))
	for index: int in range(3):
		var phase: float = fmod(progress * 3.0 + float(index) * 0.33, 1.0)
		var radius: float = 36.0 + phase * 40.0
		var alpha: float = 0.65 * (1.0 - phase)
		draw_arc(player.position, radius, 0.0, TAU, 32, Color(0.75, 0.95, 1.0, alpha), 3.0)


func _draw_spawn_flashes() -> void:
	for flash: SpawnFlash in _spawn_flashes:
		var t: float = 1.0 - (flash.time_left / ENEMY_SPAWN_FLASH_TIME)
		var radius: float = 30.0 + sin(t * TAU * 4.0) * 6.0
		var rotation: float = t * TAU * 3.0
		for arm: int in range(4):
			var angle: float = rotation + float(arm) * (TAU / 4.0)
			var endpoint: Vector2 = flash.position + Vector2(cos(angle), sin(angle)) * radius
			draw_line(flash.position, endpoint, Color(0.95, 0.95, 1.0, 0.85), 4.0)
		draw_circle(flash.position, 8.0 + sin(t * TAU * 3.0) * 3.0, Color(1.0, 1.0, 1.0, 0.9))


func _draw_powerups() -> void:
	for powerup: PowerupItem in _powerups:
		var blink: bool = fmod(powerup.blink_timer, 0.4) < 0.2
		if blink:
			continue
		var rect: Rect2 = Rect2(powerup.position - Vector2(22.0, 22.0), Vector2(44.0, 44.0))
		draw_rect(rect, Color(0.05, 0.05, 0.05, 0.85))
		draw_rect(rect, Color(1.0, 1.0, 1.0, 0.9), false, 2.0)
		var center: Vector2 = powerup.position
		match powerup.type:
			"shield":
				draw_arc(center, 11.0, PI, TAU, 16, Color(0.65, 0.95, 1.0, 1.0), 4.0)
				draw_line(center + Vector2(-11.0, 0.0), center + Vector2(11.0, 0.0), Color(0.65, 0.95, 1.0, 1.0), 4.0)
			"upgrade":
				_draw_star(center, 14.0, Color(1.0, 0.92, 0.35, 1.0))
			"wipeout":
				draw_circle(center, 9.0, Color(0.20, 0.20, 0.20, 1.0))
				draw_line(center + Vector2(0.0, -14.0), center + Vector2(0.0, -8.0), Color(0.5, 0.4, 0.2, 1.0), 4.0)
			"life":
				draw_string(ThemeDB.fallback_font, center + Vector2(-9.0, 8.0), "1P", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 22, Color(0.95, 0.85, 0.30, 1.0))
			"defence":
				draw_rect(Rect2(center + Vector2(-12.0, -3.0), Vector2(24.0, 5.0)), Color(0.55, 0.55, 0.55, 1.0))
				draw_rect(Rect2(center + Vector2(-4.0, 2.0), Vector2(8.0, 10.0)), Color(0.45, 0.30, 0.15, 1.0))
			"freeze":
				for arm: int in range(6):
					var angle: float = float(arm) * PI / 3.0
					var endpoint: Vector2 = center + Vector2(cos(angle), sin(angle)) * 12.0
					draw_line(center, endpoint, Color(0.7, 0.95, 1.0, 1.0), 3.0)


func _draw_star(center: Vector2, radius: float, color: Color) -> void:
	var points: PackedVector2Array = PackedVector2Array()
	for i: int in range(10):
		var angle: float = -PI / 2.0 + float(i) * PI / 5.0
		var r: float = radius if i % 2 == 0 else radius * 0.45
		points.append(center + Vector2(cos(angle), sin(angle)) * r)
	draw_polygon(points, PackedColorArray([color]))


func _draw_explosions() -> void:
	for explosion: Explosion in _explosions:
		var t: float = 1.0 - (explosion.time_left / explosion.lifetime)
		var max_radius: float = 40.0 if explosion.is_big else 24.0

		if t < 0.30:
			var phase: float = t / 0.30
			var radius: float = max_radius * (0.45 + phase * 0.45)
			draw_circle(explosion.position, radius, Color(1.0, 1.0, 0.95, 0.95))
			draw_circle(explosion.position, radius * 0.55, Color(1.0, 0.95, 0.55, 1.0))
		elif t < 0.62:
			var phase: float = (t - 0.30) / 0.32
			var radius: float = max_radius * (0.90 + phase * 0.30)
			var alpha: float = 1.0 - phase * 0.4
			draw_circle(explosion.position, radius, Color(1.0, 0.75, 0.20, alpha))
			draw_circle(explosion.position, radius * 0.55, Color(1.0, 0.95, 0.55, alpha))
			if explosion.is_big:
				_draw_explosion_arms(explosion.position, radius * 1.1, Color(1.0, 0.85, 0.30, alpha))
		else:
			var phase: float = (t - 0.62) / 0.38
			var radius: float = max_radius * (1.20 - phase * 0.20)
			var alpha: float = (1.0 - phase) * 0.85
			if explosion.is_big:
				_draw_explosion_smoke_chunks(explosion.position, radius, alpha)
			else:
				draw_circle(explosion.position, radius * 0.6, Color(0.6, 0.55, 0.5, alpha))


func _draw_explosion_arms(center: Vector2, length: float, color: Color) -> void:
	for arm: int in range(4):
		var angle: float = float(arm) * PI * 0.5
		var dir: Vector2 = Vector2(cos(angle), sin(angle))
		draw_line(center + dir * length * 0.3, center + dir * length, color, 4.0)
	for arm: int in range(4):
		var angle: float = float(arm) * PI * 0.5 + PI * 0.25
		var dir: Vector2 = Vector2(cos(angle), sin(angle))
		draw_line(center + dir * length * 0.4, center + dir * length * 0.85, color, 2.5)


func _draw_explosion_smoke_chunks(center: Vector2, radius: float, alpha: float) -> void:
	var chunk_color: Color = Color(0.55, 0.50, 0.45, alpha)
	var dark_color: Color = Color(0.30, 0.27, 0.24, alpha)
	for i: int in range(6):
		var angle: float = float(i) * PI * 2.0 / 6.0
		var offset: Vector2 = Vector2(cos(angle), sin(angle)) * radius * 0.55
		draw_circle(center + offset, radius * 0.34, chunk_color)
		draw_circle(center + offset * 0.55, radius * 0.30, dark_color)


func _draw_score_popups() -> void:
	for popup: ScorePopup in _score_popups:
		var alpha: float = clampf(popup.time_left / SCORE_POPUP_TIME, 0.0, 1.0)
		draw_string(ThemeDB.fallback_font, popup.position, popup.text, HORIZONTAL_ALIGNMENT_CENTER, -1.0, 16, Color(1.0, 1.0, 1.0, alpha))


func _draw_overlay_message() -> void:
	if not _game_over and not _stage_clear:
		return
	if _stage_clear:
		_draw_bonus_screen()
		return

	var scroll_duration: float = 1.4
	var t: float = clampf(_game_over_progress / scroll_duration, 0.0, 1.0)
	var start_y: float = BOARD_ORIGIN.y + BOARD_SIZE.y - 12.0
	var end_y: float = BOARD_ORIGIN.y + BOARD_SIZE.y * 0.5 - 24.0
	var current_y: float = lerpf(start_y, end_y, _ease_out_cubic(t))
	var overlay_alpha: float = 0.62 * t
	if overlay_alpha > 0.01:
		var overlay: Rect2 = Rect2(BOARD_ORIGIN, BOARD_SIZE)
		draw_rect(overlay, Color(0.0, 0.0, 0.0, overlay_alpha))
	var center_x: float = BOARD_ORIGIN.x + BOARD_SIZE.x * 0.5
	draw_string(ThemeDB.fallback_font, Vector2(center_x - 124.0, current_y), "GAME", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 48, Color(0.85, 0.18, 0.10, 1.0))
	draw_string(ThemeDB.fallback_font, Vector2(center_x + 4.0, current_y), "OVER", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 48, Color(0.85, 0.18, 0.10, 1.0))
	if t >= 1.0:
		var blink: bool = fmod(_game_over_progress * 2.0, 1.0) < 0.6
		if blink:
			draw_string(ThemeDB.fallback_font, Vector2(center_x - 130.0, current_y + 60.0), "SPACE / Enter - menu", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 18, Color(0.92, 0.92, 0.82, 1.0))


func _ease_out_cubic(t: float) -> float:
	var inv: float = 1.0 - t
	return 1.0 - inv * inv * inv


func _draw_bonus_screen() -> void:
	var overlay: Rect2 = Rect2(BOARD_ORIGIN, BOARD_SIZE)
	draw_rect(overlay, Color(0.0, 0.0, 0.0, 0.78))
	var origin: Vector2 = BOARD_ORIGIN + Vector2(140.0, 130.0)
	draw_string(ThemeDB.fallback_font, origin, "STAGE %d CLEAR" % [_stage_index + 1], HORIZONTAL_ALIGNMENT_LEFT, -1.0, 36, Color(1.0, 0.85, 0.30, 1.0))
	draw_string(ThemeDB.fallback_font, origin + Vector2(0.0, 44.0), "HI-SCORE %d" % _high_score, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 20, Color.WHITE)
	for player_idx: int in range(_players.size()):
		draw_string(ThemeDB.fallback_font, origin + Vector2(float(player_idx) * 200.0, 76.0), "P%d %d" % [player_idx + 1, _players[player_idx].score], HORIZONTAL_ALIGNMENT_LEFT, -1.0, 20, Color.WHITE)

	var rows: Array[Array] = [
		["BASIC", _kills_by_tier["a"], 100],
		["FAST", _kills_by_tier["b"], 200],
		["POWER", _kills_by_tier["c"], 300],
		["ARMOR", _kills_by_tier["d"], 400],
	]
	var total_pts: int = 0
	var total_kills: int = 0
	var y: float = 130.0
	for row: Array in rows:
		var name: String = row[0]
		var count: int = int(row[1])
		var per: int = int(row[2])
		var pts: int = count * per
		total_pts += pts
		total_kills += count
		draw_string(ThemeDB.fallback_font, origin + Vector2(0.0, y), name, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 20, Color.WHITE)
		draw_string(ThemeDB.fallback_font, origin + Vector2(160.0, y), "%d  x  %d" % [per, count], HORIZONTAL_ALIGNMENT_LEFT, -1.0, 20, Color.WHITE)
		draw_string(ThemeDB.fallback_font, origin + Vector2(320.0, y), "= %d" % pts, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 20, Color.WHITE)
		y += 30.0
	draw_line(origin + Vector2(0.0, y - 4.0), origin + Vector2(420.0, y - 4.0), Color.WHITE, 1.0)
	draw_string(ThemeDB.fallback_font, origin + Vector2(0.0, y + 24.0), "TOTAL %d (%d enemies)" % [total_pts, total_kills], HORIZONTAL_ALIGNMENT_LEFT, -1.0, 22, Color(1.0, 0.85, 0.30, 1.0))
	if _bonus_screen_time <= 0.0:
		draw_string(ThemeDB.fallback_font, origin + Vector2(0.0, y + 80.0), "SPACE / Enter - next stage", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 18, Color(0.92, 0.92, 0.82, 1.0))


func _draw_pause_overlay() -> void:
	var overlay: Rect2 = Rect2(BOARD_ORIGIN, BOARD_SIZE)
	draw_rect(overlay, Color(0.0, 0.0, 0.0, 0.45))
	draw_string(ThemeDB.fallback_font, BOARD_ORIGIN + Vector2(BOARD_SIZE.x * 0.5 - 70.0, BOARD_SIZE.y * 0.5), "PAUSE", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 56, Color.WHITE)


func _update_status() -> void:
	if _status_label == null:
		return
	var p1_lives: int = _players[0].lives if _players.size() > 0 else 0
	var p1_bullets: int = 0
	var p1_cd: float = 0.0
	var p1_pos: Vector2 = Vector2.ZERO
	var p1_dir: Vector2i = Vector2i.ZERO
	if _players.size() > 0:
		p1_bullets = _count_player_bullets(_players[0])
		p1_cd = _players[0].shoot_cooldown
		p1_pos = _players[0].position
		p1_dir = _players[0].direction
	_status_label.text = "St %d  hi %d  P1 lives %d  enemies %d/%d  bullets %d  cd %.2f  pos (%.0f,%.0f)  dir (%d,%d)" % [
		_stage_index + 1, _high_score, p1_lives, _kills, ENEMIES_PER_STAGE, p1_bullets, p1_cd, p1_pos.x, p1_pos.y, p1_dir.x, p1_dir.y
	]


# ============================================================================
# GRID HELPERS
# ============================================================================
func _set_macro_tile(cell: Vector2i, tile: int) -> void:
	for offset_y: int in range(DESTRUCTION_SUBDIVISIONS):
		for offset_x: int in range(DESTRUCTION_SUBDIVISIONS):
			_set_tile(Vector2i(cell.x * DESTRUCTION_SUBDIVISIONS + offset_x, cell.y * DESTRUCTION_SUBDIVISIONS + offset_y), tile)


func _set_tile(cell: Vector2i, tile: int) -> void:
	if cell.x < 0 or cell.y < 0 or cell.x >= DESTRUCTION_GRID_COLUMNS or cell.y >= DESTRUCTION_GRID_ROWS:
		return
	_tiles[cell.y][cell.x] = tile


func _tile_at_cell(cell: Vector2i) -> int:
	if cell.x < 0 or cell.y < 0 or cell.x >= DESTRUCTION_GRID_COLUMNS or cell.y >= DESTRUCTION_GRID_ROWS:
		return TILE_STEEL
	return int(_tiles[cell.y][cell.x])


func _tile_at_world(position: Vector2) -> int:
	return _tile_at_cell(_world_to_destruction_cell(position))


func _tank_cell_center(cell: Vector2i) -> Vector2:
	# `cell` is the top-left of a 2x2-macro tank footprint (matches cattle-bity sub-tile coords).
	# Return the CENTER of that footprint so render and collision use the same anchor.
	return BOARD_ORIGIN + Vector2(cell * CELL_SIZE) + Vector2(CELL_SIZE, CELL_SIZE)


func _cell_top_left(cell: Vector2i) -> Vector2:
	return BOARD_ORIGIN + Vector2(cell * CELL_SIZE)


func _destruction_cell_top_left(cell: Vector2i) -> Vector2:
	return BOARD_ORIGIN + Vector2(cell * DESTRUCTION_CELL_SIZE)


func _world_to_destruction_cell(position: Vector2) -> Vector2i:
	var local_position: Vector2 = position - BOARD_ORIGIN
	return Vector2i(int(floorf(local_position.x / DESTRUCTION_CELL_SIZE)), int(floorf(local_position.y / DESTRUCTION_CELL_SIZE)))


func _is_base_subcell(cell: Vector2i) -> bool:
	return cell.x >= 24 and cell.x <= 27 and cell.y >= 48 and cell.y <= 51


func _point_inside_board(position: Vector2) -> bool:
	return position.x >= BOARD_ORIGIN.x and position.y >= BOARD_ORIGIN.y and position.x <= BOARD_ORIGIN.x + BOARD_SIZE.x and position.y <= BOARD_ORIGIN.y + BOARD_SIZE.y


# ============================================================================
# MENU
# ============================================================================
func _enter_menu() -> void:
	_game_state = STATE_MENU
	_menu_repeat_timer = 0.0
	_menu_index = 0
	_paused = false


func _update_menu(delta: float) -> void:
	_menu_repeat_timer = maxf(0.0, _menu_repeat_timer - delta)
	if _menu_repeat_timer <= 0.0:
		if Input.is_key_pressed(KEY_UP) or Input.is_key_pressed(KEY_W):
			_menu_index = (_menu_index - 1 + MENU_OPTIONS.size()) % MENU_OPTIONS.size()
			_menu_repeat_timer = 0.18
			_play_sfx("menu_move")
		elif Input.is_key_pressed(KEY_DOWN) or Input.is_key_pressed(KEY_S):
			_menu_index = (_menu_index + 1) % MENU_OPTIONS.size()
			_menu_repeat_timer = 0.18
			_play_sfx("menu_move")

	if Input.is_action_just_pressed("ui_accept"):
		match _menu_index:
			0:
				_two_players = false
				_start_new_campaign()
			1:
				_two_players = true
				_start_new_campaign()
			2:
				_start_construction()


func _draw_menu() -> void:
	var center_x: float = SCREEN_SIZE.x * 0.5
	draw_string(ThemeDB.fallback_font, Vector2(center_x - 140.0, 200.0), "TANKS", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 96, Color(1.0, 0.93, 0.45, 1.0))
	draw_string(ThemeDB.fallback_font, Vector2(center_x - 156.0, 254.0), "BATTLE CITY", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 40, Color(0.95, 0.85, 0.30, 1.0))
	draw_string(ThemeDB.fallback_font, Vector2(center_x - 116.0, 308.0), "HI-SCORE %d" % _high_score, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 24, Color.WHITE)

	var origin_y: float = 420.0
	for i: int in range(MENU_OPTIONS.size()):
		var text: String = MENU_OPTIONS[i]
		if i == 2 and _has_custom_layout:
			text += "  *"
		var color: Color = Color(1.0, 1.0, 0.65, 1.0) if i == _menu_index else Color(0.85, 0.85, 0.85, 1.0)
		var label_x: float = center_x - 90.0
		draw_string(ThemeDB.fallback_font, Vector2(label_x, origin_y + float(i) * 64.0), text, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 32, color)
		if i == _menu_index:
			var cursor_x: float = label_x - 60.0
			var cursor_y: float = origin_y + float(i) * 64.0 - 28.0
			_draw_tank(Vector2(cursor_x, cursor_y + 16.0), Vector2i.RIGHT, COLOR_PLAYER, false)

	draw_string(ThemeDB.fallback_font, Vector2(center_x - 230.0, 760.0), "Up/Down to choose, Space/Enter to confirm", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 18, Color(0.85, 0.85, 0.85, 1.0))
	draw_string(ThemeDB.fallback_font, Vector2(center_x - 230.0, 786.0), "Esc to exit a stage back to this menu", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 16, Color(0.65, 0.65, 0.65, 1.0))


# ============================================================================
# CONSTRUCTION MODE
# ============================================================================
func _start_construction() -> void:
	_game_state = STATE_CONSTRUCTION
	if _editor_layout.size() != GRID_ROWS:
		_editor_layout.clear()
		var source: Array = _custom_layout_or_blank()
		for row: String in source:
			_editor_layout.append(row)
	_editor_cursor = Vector2i(GRID_COLUMNS / 2, GRID_ROWS / 2)
	_editor_brush_index = 1
	_editor_repeat_timer = 0.0
	_editor_message = ""
	_editor_message_time = 0.0


func _custom_layout_or_blank() -> Array:
	if _has_custom_layout and _custom_layout_cached.size() == GRID_ROWS:
		return _custom_layout_cached
	var blank: Array = []
	for _i: int in range(GRID_ROWS):
		blank.append(".".repeat(GRID_COLUMNS))
	return blank


var _custom_layout_cached: Array = []


func _update_construction(delta: float) -> void:
	_editor_repeat_timer = maxf(0.0, _editor_repeat_timer - delta)
	_editor_message_time = maxf(0.0, _editor_message_time - delta)

	if Input.is_action_just_pressed("ui_cancel"):
		_enter_menu()
		return

	if _editor_repeat_timer <= 0.0:
		var moved: bool = false
		if Input.is_key_pressed(KEY_LEFT) or Input.is_key_pressed(KEY_A):
			_editor_cursor.x = clampi(_editor_cursor.x - 1, 0, GRID_COLUMNS - 1)
			moved = true
		elif Input.is_key_pressed(KEY_RIGHT) or Input.is_key_pressed(KEY_D):
			_editor_cursor.x = clampi(_editor_cursor.x + 1, 0, GRID_COLUMNS - 1)
			moved = true
		elif Input.is_key_pressed(KEY_UP) or Input.is_key_pressed(KEY_W):
			_editor_cursor.y = clampi(_editor_cursor.y - 1, 0, GRID_ROWS - 1)
			moved = true
		elif Input.is_key_pressed(KEY_DOWN) or Input.is_key_pressed(KEY_S):
			_editor_cursor.y = clampi(_editor_cursor.y + 1, 0, GRID_ROWS - 1)
			moved = true
		if moved:
			_editor_repeat_timer = EDITOR_REPEAT_DELAY

	for digit_i: int in range(BRUSH_TILES.size()):
		var key_code: int = KEY_1 + digit_i
		if Input.is_key_pressed(key_code):
			_editor_brush_index = digit_i

	if Input.is_key_pressed(KEY_SPACE):
		_set_editor_cell(_editor_cursor, BRUSH_TILES[_editor_brush_index])

	if Input.is_action_just_pressed("ui_accept"):
		_save_custom_layout()
		_play_custom_level()
		return

	if Input.is_key_pressed(KEY_F):
		var brush_char: String = _char_from_tile(BRUSH_TILES[_editor_brush_index])
		var filled: String = brush_char.repeat(GRID_COLUMNS)
		for y: int in range(GRID_ROWS):
			_editor_layout[y] = filled
		_editor_message = "FILLED"
		_editor_message_time = 1.0


func _set_editor_cell(cell: Vector2i, tile: int) -> void:
	if cell.x < 0 or cell.y < 0 or cell.x >= GRID_COLUMNS or cell.y >= GRID_ROWS:
		return
	var row: String = _editor_layout[cell.y]
	var ch: String = _char_from_tile(tile)
	_editor_layout[cell.y] = row.substr(0, cell.x) + ch + row.substr(cell.x + 1)


func _char_from_tile(tile: int) -> String:
	match tile:
		TILE_BRICK: return "B"
		TILE_STEEL: return "S"
		TILE_WATER: return "W"
		TILE_FOREST: return "F"
		TILE_ICE: return "I"
		_: return "."


func _play_custom_level() -> void:
	_custom_layout_cached = _editor_layout.duplicate()
	_has_custom_layout = true
	_two_players = false
	_players.clear()
	_players.append(PlayerSlot.new(0))
	_stage_index = -1
	_load_custom_into_stage()
	_editor_message = "SAVED, TESTING"


func _load_custom_into_stage() -> void:
	_game_state = STATE_PLAYING
	for player: PlayerSlot in _players:
		player.position = _tank_cell_center(player.spawn_cell)
		player.direction = Vector2i.UP
		player.queued_turn = Vector2i.ZERO
		player.shoot_cooldown = 0.0
		player.invulnerable_timer = RESPAWN_INVULNERABLE_TIME
		player.alive = player.lives > 0
	_kills = 0
	_kills_by_tier = {"a": 0, "b": 0, "c": 0, "d": 0}
	_spawned_enemies = 0
	_enemy_spawn_index = 0
	_enemy_spawn_timer = ENEMY_FIRST_SPAWN_DELAY
	_base_alive = true
	_base_defence_time_left = 0.0
	_freeze_time_left = 0.0
	_game_over = false
	_stage_clear = false
	_bonus_screen_time = 0.0
	_paused = false
	_enemies.clear()
	_bullets.clear()
	_spawn_flashes.clear()
	_powerups.clear()
	_explosions.clear()
	_score_popups.clear()
	_particles.clear()
	_shake_time_left = 0.0
	_stage_intro_time_left = STAGE_INTRO_TIME
	_play_sfx("stage_start")
	_load_custom_layout_into_tiles()
	_protect_base_with_brick()
	_clear_spawn_zones()


func _load_custom_layout_into_tiles() -> void:
	_tiles.clear()
	for sub_y: int in range(DESTRUCTION_GRID_ROWS):
		var sub_row: Array[int] = []
		for sub_x: int in range(DESTRUCTION_GRID_COLUMNS):
			sub_row.append(TILE_EMPTY)
		_tiles.append(sub_row)
	for y: int in range(GRID_ROWS):
		var line: String = _custom_layout_cached[y]
		for x: int in range(GRID_COLUMNS):
			_set_macro_tile(Vector2i(x, y), _tile_from_char(line.substr(x, 1)))


func _draw_construction() -> void:
	draw_rect(Rect2(Vector2.ZERO, SCREEN_SIZE), COLOR_PANEL)
	draw_rect(Rect2(BOARD_ORIGIN, BOARD_SIZE), COLOR_BOARD)

	for y: int in range(GRID_ROWS):
		var line: String = _editor_layout[y]
		for x: int in range(GRID_COLUMNS):
			var ch: String = line.substr(x, 1)
			var tile: int = _tile_from_char(ch)
			if tile == TILE_EMPTY:
				continue
			_draw_editor_macro(Vector2i(x, y), tile)
	_draw_editor_base_marker()
	_draw_editor_spawn_markers()
	_draw_editor_cursor()
	_draw_construction_panels()


func _draw_editor_macro(cell: Vector2i, tile: int) -> void:
	var rect: Rect2 = Rect2(_cell_top_left(cell), Vector2(CELL_SIZE, CELL_SIZE))
	match tile:
		TILE_BRICK:
			draw_rect(rect.grow(-1.0), COLOR_BRICK)
			draw_rect(rect.grow(-2.0), Color(0.28, 0.12, 0.06, 0.55), false, 1.0)
		TILE_STEEL:
			draw_rect(rect.grow(-1.0), COLOR_STEEL)
		TILE_WATER:
			draw_rect(rect.grow(-1.0), COLOR_WATER)
		TILE_FOREST:
			draw_rect(rect.grow(-1.0), COLOR_FOREST)
		TILE_ICE:
			draw_rect(rect.grow(-1.0), COLOR_ICE)


func _draw_editor_base_marker() -> void:
	var top_left: Vector2 = _cell_top_left(Vector2i(12, 24))
	draw_rect(Rect2(top_left + Vector2(2.0, 2.0), Vector2(CELL_SIZE * 2 - 4, CELL_SIZE * 2 - 4)), Color(0.55, 0.45, 0.20, 0.55))


func _draw_editor_spawn_markers() -> void:
	for cell: Vector2i in SPAWN_CELLS:
		var pos: Vector2 = _tank_cell_center(cell)
		draw_arc(pos, 16.0, 0.0, TAU, 16, Color(0.95, 0.45, 0.45, 0.85), 2.0)
	var p1: Vector2 = _tank_cell_center(PLAYER_SPAWN_CELLS[0])
	draw_arc(p1, 16.0, 0.0, TAU, 16, PLAYER_COLORS[0], 2.0)
	var p2: Vector2 = _tank_cell_center(PLAYER_SPAWN_CELLS[1])
	draw_arc(p2, 16.0, 0.0, TAU, 16, PLAYER_COLORS[1], 2.0)


func _draw_editor_cursor() -> void:
	var top_left: Vector2 = _cell_top_left(_editor_cursor)
	var rect: Rect2 = Rect2(top_left, Vector2(CELL_SIZE, CELL_SIZE))
	var phase: float = fmod(Time.get_ticks_msec() / 200.0, 2.0)
	var alpha: float = 1.0 if phase < 1.0 else 0.45
	draw_rect(rect, Color(1.0, 1.0, 1.0, alpha), false, 2.0)
	var preview: Rect2 = rect.grow(-3.0)
	var brush_color: Color = _color_for_brush(BRUSH_TILES[_editor_brush_index])
	draw_rect(preview, brush_color)


func _color_for_brush(tile: int) -> Color:
	match tile:
		TILE_BRICK: return COLOR_BRICK
		TILE_STEEL: return COLOR_STEEL
		TILE_WATER: return COLOR_WATER
		TILE_FOREST: return COLOR_FOREST
		TILE_ICE: return COLOR_ICE
		_: return Color(0.05, 0.05, 0.05, 0.6)


func _draw_construction_panels() -> void:
	var lx: float = 24.0
	draw_string(ThemeDB.fallback_font, Vector2(lx, 70.0), "BUILD", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 36, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(lx, 102.0), "MODE", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 26, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(lx, 148.0), "Cursor: arrows / WASD", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(lx, 168.0), "Place: Space (hold)", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(lx, 188.0), "Brush: 1..6", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(lx, 208.0), "F: fill all", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(lx, 228.0), "Enter: save & test", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)
	draw_string(ThemeDB.fallback_font, Vector2(lx, 248.0), "Esc: back to menu", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 13, Color.BLACK)

	var palette_y: float = 290.0
	for i: int in range(BRUSH_TILES.size()):
		var rect: Rect2 = Rect2(Vector2(lx, palette_y + float(i) * 38.0), Vector2(30.0, 30.0))
		draw_rect(rect, _color_for_brush(BRUSH_TILES[i]))
		var label_color: Color = Color(0.95, 0.95, 0.30, 1.0) if i == _editor_brush_index else Color.BLACK
		draw_string(ThemeDB.fallback_font, Vector2(lx + 44.0, palette_y + float(i) * 38.0 + 22.0), "%d  %s" % [i + 1, BRUSH_NAMES[i]], HORIZONTAL_ALIGNMENT_LEFT, -1.0, 16, label_color)

	if _editor_message != "" and _editor_message_time > 0.0:
		draw_string(ThemeDB.fallback_font, Vector2(BOARD_ORIGIN.x + 8.0, BOARD_ORIGIN.y + BOARD_SIZE.y + 28.0), _editor_message, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 18, Color(0.95, 0.95, 0.30, 1.0))


# ============================================================================
# PERSISTENCE
# ============================================================================
func _save_high_score() -> void:
	var config: ConfigFile = ConfigFile.new()
	config.set_value("score", "hi", _high_score)
	config.save(HIGHSCORE_PATH)


func _load_high_score() -> void:
	var config: ConfigFile = ConfigFile.new()
	var err: int = config.load(HIGHSCORE_PATH)
	if err == OK:
		_high_score = int(config.get_value("score", "hi", DEFAULT_HIGHSCORE))
	else:
		_high_score = DEFAULT_HIGHSCORE


func _save_custom_layout() -> void:
	var file: FileAccess = FileAccess.open(CUSTOM_LEVEL_PATH, FileAccess.WRITE)
	if file == null:
		return
	var data: Dictionary = {"layout": _editor_layout}
	file.store_string(JSON.stringify(data))
	file.close()
	_has_custom_layout = true
	_custom_layout_cached = _editor_layout.duplicate()


func _load_custom_layout() -> void:
	var file: FileAccess = FileAccess.open(CUSTOM_LEVEL_PATH, FileAccess.READ)
	if file == null:
		_has_custom_layout = false
		return
	var contents: String = file.get_as_text()
	file.close()
	var parsed: Variant = JSON.parse_string(contents)
	if parsed is Dictionary and parsed.has("layout"):
		var layout: Array = parsed["layout"]
		if layout.size() == GRID_ROWS:
			_custom_layout_cached.clear()
			for row: Variant in layout:
				_custom_layout_cached.append(String(row))
			_has_custom_layout = true
			return
	_has_custom_layout = false


# ============================================================================
# JUICE FX (screen shake, sparks, stage curtain)
# ============================================================================
func _add_shake(strength: float, duration: float) -> void:
	if strength >= _shake_strength or _shake_time_left <= 0.0:
		_shake_strength = strength
		_shake_duration = maxf(duration, 0.001)
		_shake_time_left = duration


func _spawn_sparks(origin: Vector2, incoming_direction: Vector2, count: int, color: Color, speed: float) -> void:
	for _i: int in range(count):
		var particle: Particle = Particle.new()
		particle.position = origin
		var direction: Vector2
		if incoming_direction == Vector2.ZERO:
			direction = Vector2.RIGHT.rotated(randf_range(0.0, TAU))
		else:
			direction = (-incoming_direction).rotated(randf_range(-1.1, 1.1))
		particle.velocity = direction * speed * randf_range(0.5, 1.4)
		particle.lifetime = randf_range(0.14, 0.34)
		particle.time_left = particle.lifetime
		particle.color = color
		particle.size = randf_range(2.5, 5.0)
		_particles.append(particle)


func _spawn_muzzle_flash(origin: Vector2, forward: Vector2) -> void:
	var flash: Particle = Particle.new()
	flash.position = origin
	flash.velocity = forward * 60.0
	flash.lifetime = 0.07
	flash.time_left = flash.lifetime
	flash.color = Color(1.0, 0.95, 0.60, 1.0)
	flash.size = 12.0
	_particles.append(flash)
	_spawn_sparks(origin, -forward, 3, Color(1.0, 0.85, 0.40, 1.0), 130.0)


func _update_particles(delta: float) -> void:
	for index: int in range(_particles.size() - 1, -1, -1):
		var particle: Particle = _particles[index]
		particle.time_left -= delta
		if particle.time_left <= 0.0:
			_particles.remove_at(index)
			continue
		particle.position += particle.velocity * delta
		particle.velocity *= pow(0.02, delta)


func _draw_particles() -> void:
	for particle: Particle in _particles:
		var alpha: float = clampf(particle.time_left / particle.lifetime, 0.0, 1.0)
		var half: float = particle.size * 0.5 * (0.5 + alpha * 0.5)
		var color: Color = particle.color
		color.a *= alpha
		draw_rect(Rect2(particle.position - Vector2(half, half), Vector2(half * 2.0, half * 2.0)), color)


func _draw_stage_intro() -> void:
	var open_phase: float = 0.0
	if _stage_intro_time_left < STAGE_INTRO_OPEN_TIME:
		open_phase = 1.0 - _stage_intro_time_left / STAGE_INTRO_OPEN_TIME
	var half_height: float = SCREEN_SIZE.y * 0.5
	var shift: float = half_height * _ease_out_cubic(open_phase)
	draw_rect(Rect2(Vector2(0.0, -shift), Vector2(SCREEN_SIZE.x, half_height)), COLOR_PANEL)
	draw_rect(Rect2(Vector2(0.0, half_height + shift), Vector2(SCREEN_SIZE.x, half_height)), COLOR_PANEL)
	if open_phase < 0.4:
		var text: String = "CUSTOM STAGE" if _stage_index < 0 else "STAGE %d" % (_stage_index + 1)
		var alpha: float = 1.0 - open_phase / 0.4
		draw_string(ThemeDB.fallback_font, Vector2(SCREEN_SIZE.x * 0.5 - 90.0, half_height - shift - 10.0), text, HORIZONTAL_ALIGNMENT_LEFT, -1.0, 34, Color(0.0, 0.0, 0.0, alpha))


func _draw_water_macro(cell: Vector2i) -> void:
	var rect: Rect2 = Rect2(_cell_top_left(cell), Vector2(CELL_SIZE, CELL_SIZE))
	draw_rect(rect.grow(-1.0), COLOR_WATER)
	var t: float = Time.get_ticks_msec() / 1000.0
	var phase: float = t * 1.6 + float(cell.x) * 0.73 + float(cell.y) * 1.31
	var wave_y: float = rect.position.y + 4.0 + fmod(phase, 1.0) * (CELL_SIZE - 8.0)
	var alpha: float = 0.30 + 0.20 * sin(phase * TAU)
	draw_line(Vector2(rect.position.x + 5.0, wave_y), Vector2(rect.position.x + CELL_SIZE - 5.0, wave_y), Color(0.45, 0.68, 1.0, alpha), 2.0)


func _draw_base_rubble() -> void:
	var left: Vector2 = _cell_top_left(Vector2i(12, 24))
	var size: float = float(CELL_SIZE) * 2.0
	var center: Vector2 = left + Vector2(size * 0.5, size * 0.5)
	draw_rect(Rect2(left + Vector2(2.0, 2.0), Vector2(size - 4.0, size - 4.0)), Color(0.22, 0.20, 0.18, 1.0))
	var rubble: Color = Color(0.42, 0.40, 0.36, 1.0)
	draw_circle(center + Vector2(-14.0, 12.0), 11.0, rubble)
	draw_circle(center + Vector2(10.0, 16.0), 9.0, rubble)
	draw_circle(center + Vector2(2.0, 2.0), 12.0, Color(0.33, 0.30, 0.27, 1.0))
	draw_line(center + Vector2(-18.0, 6.0), center + Vector2(20.0, -14.0), Color(0.15, 0.13, 0.11, 1.0), 3.0)


# ============================================================================
# SOUND (procedural 8-bit-style SFX, generated at startup — no asset files)
# ============================================================================
func _setup_sfx() -> void:
	_sfx_streams["shoot"] = _make_wav(_synth_sweep(0.09, 880.0, 380.0, 0.22, true))
	_sfx_streams["brick"] = _make_wav(_synth_noise(0.10, 0.45, 0.30, 1.4))
	_sfx_streams["steel"] = _make_wav(_synth_sweep(0.06, 1500.0, 1150.0, 0.18, false))
	_sfx_streams["armor_hit"] = _make_wav(_synth_sweep(0.07, 320.0, 190.0, 0.30, true))
	_sfx_streams["explosion"] = _make_wav(_synth_noise(0.38, 0.70, 0.16, 1.8))
	_sfx_streams["explosion_big"] = _make_wav(_synth_noise(0.75, 0.85, 0.09, 2.2))
	_sfx_streams["powerup_spawn"] = _make_wav(_synth_notes([1047.0, 1319.0], 0.09, 0.22))
	_sfx_streams["pickup"] = _make_wav(_synth_notes([784.0, 988.0, 1319.0, 1568.0], 0.07, 0.22))
	_sfx_streams["life"] = _make_wav(_synth_notes([660.0, 784.0, 988.0, 1319.0, 1568.0], 0.08, 0.24))
	_sfx_streams["freeze"] = _make_wav(_synth_sweep(0.35, 1250.0, 240.0, 0.22, false))
	_sfx_streams["stage_start"] = _make_wav(_synth_notes([392.0, 523.0, 659.0, 784.0], 0.11, 0.24))
	_sfx_streams["stage_clear"] = _make_wav(_synth_notes([523.0, 659.0, 784.0, 1047.0], 0.10, 0.24))
	_sfx_streams["game_over"] = _make_wav(_synth_notes([523.0, 494.0, 440.0, 392.0, 349.0, 330.0], 0.14, 0.24))
	_sfx_streams["menu_move"] = _make_wav(_synth_sweep(0.04, 1000.0, 900.0, 0.16, true))
	for _i: int in range(SFX_POOL_SIZE):
		var player: AudioStreamPlayer = AudioStreamPlayer.new()
		add_child(player)
		_sfx_players.append(player)


func _play_sfx(sfx_name: String, volume_db: float = 0.0) -> void:
	if not _sfx_streams.has(sfx_name):
		return
	var target: AudioStreamPlayer = null
	for player: AudioStreamPlayer in _sfx_players:
		if not player.playing:
			target = player
			break
	if target == null:
		target = _sfx_players[0]
	target.stream = _sfx_streams[sfx_name]
	target.volume_db = volume_db
	target.play()


func _make_wav(samples: PackedFloat32Array) -> AudioStreamWAV:
	var bytes: PackedByteArray = PackedByteArray()
	bytes.resize(samples.size() * 2)
	for i: int in range(samples.size()):
		bytes.encode_s16(i * 2, clampi(int(samples[i] * 32767.0), -32768, 32767))
	var wav: AudioStreamWAV = AudioStreamWAV.new()
	wav.format = AudioStreamWAV.FORMAT_16_BITS
	wav.mix_rate = SFX_RATE
	wav.stereo = false
	wav.data = bytes
	return wav


func _synth_sweep(duration: float, freq_from: float, freq_to: float, amplitude: float, square: bool) -> PackedFloat32Array:
	var count: int = int(duration * SFX_RATE)
	var samples: PackedFloat32Array = PackedFloat32Array()
	samples.resize(count)
	var phase: float = 0.0
	for i: int in range(count):
		var t: float = float(i) / float(count)
		phase += lerpf(freq_from, freq_to, t) / float(SFX_RATE)
		var wave: float = sin(phase * TAU)
		if square:
			wave = 1.0 if wave >= 0.0 else -1.0
		samples[i] = wave * amplitude * (1.0 - t)
	return samples


func _synth_noise(duration: float, amplitude: float, lowpass: float, decay_power: float) -> PackedFloat32Array:
	var count: int = int(duration * SFX_RATE)
	var samples: PackedFloat32Array = PackedFloat32Array()
	samples.resize(count)
	var value: float = 0.0
	for i: int in range(count):
		var t: float = float(i) / float(count)
		value = lerpf(value, randf_range(-1.0, 1.0), lowpass)
		samples[i] = value * amplitude * pow(1.0 - t, decay_power)
	return samples


func _synth_notes(frequencies: Array, note_duration: float, amplitude: float) -> PackedFloat32Array:
	var per_note: int = int(note_duration * SFX_RATE)
	var samples: PackedFloat32Array = PackedFloat32Array()
	samples.resize(per_note * frequencies.size())
	var phase: float = 0.0
	for note_index: int in range(frequencies.size()):
		var freq: float = float(frequencies[note_index])
		for i: int in range(per_note):
			var t: float = float(i) / float(per_note)
			phase += freq / float(SFX_RATE)
			var wave: float = 1.0 if sin(phase * TAU) >= 0.0 else -1.0
			var envelope: float = minf(1.0, t * 12.0) * (1.0 - t * 0.7)
			samples[note_index * per_note + i] = wave * amplitude * envelope
	return samples
