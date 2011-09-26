-- fills the apple cart with 5 apples
-- it could probably hold more, but they tend to spill really easily
function applecart(lthing)
	spawnMoreApples(lthing, -1, 2, 0, 1, 5, 1)
	--deactivateThing(lthing)
end

-- normally this could hold around 60 apples, but that really slowed down the game, so now it only has 16
function containerLarge(lthing)
	spawnMoreApples(lthing, -1.14589, 2.02102, -1.11799, 4, 1, 4)
	deactivateThing(lthing)
end

-- this could hold around 20ish but that slowed down the game so now it only has 9
function containerSmall(lthing)
	spawnMoreApples(lthing, -0.662419, 1.5571, -0.639155, 3, 1, 3)
	deactivateThing(lthing)
end
