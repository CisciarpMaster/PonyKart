luanet.load_assembly("Lymph")

-- this gets run right when our level finishes initialiation - if we want to run specific per-level stuff, that should go right here!
-- the file name does not matter, but the function MUST have the same name as the level, the file MUST be in media/scripts/yourLevelNameHere, and the function must have "level" as a parameter!
function shittyterrain(level)
	print("Test of startup level script successful for " .. level.Name .. "!")
end