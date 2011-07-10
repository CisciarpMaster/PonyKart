-- first we tell lua about our c# program
luanet.load_assembly("Lymph")

-- then we tell it what things we're going to be using
--levelman = luanet.import_type("Lymph.Lua.LevelManagerWrapper")
--triggerw = luanet.import_type("Lymph.Lua.TriggerWrapper")

print("test.lua has been run")
-- then we do stuff!
--print("lets start level 4")
--levelman.loadLevelByName("Vessel")
--LevelManagerWrapper.hookScriptToLevelLoadEvent("media/scripts/testevent.lua")
--LevelManagerWrapper.hookFunctionToLevelLoadEvent(testfunction)

function testfunctionn(s1, s2, tf)
	if Triggers.isEnterFlag(tf) then
		print("enter")
	else
		print("leave")
	end
	print(s2.Actor.Name)
end

Triggers.hookScriptToTriggerArea("test trigger area", "media/scripts/testevent.lua")
Triggers.hookFunctionToTriggerArea("test trigger area", testfunctionn)



function testfunction(e)
	print("=== function event hook test successful!")
end