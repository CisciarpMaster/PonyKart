-- fills the apple cart with 5 apples
-- it could probably hold more, but it tends to spill really easily
function applecart(lthing)
	spawnMoreApples(lthing, -0.2, 0.4, 0, 1, 5, 1)
end

-- normally this could hold around 60 apples, but that really slowed down the game, so now it only has 16
function appleContainerLarge(lthing)
	spawnMoreApples(lthing, -0.229178, 0.404204, -0.223598, 4, 1, 4)
end

-- this could hold around 20ish but that slowed down the game so now it only has 9
function appleContainerSmall(lthing)
	spawnMoreApples(lthing, -0.1324838, 0.31142, -0.127831, 3, 1, 3)
end
