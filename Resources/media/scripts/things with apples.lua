-- fills the apple cart with apples
function applecart(lthing)
	spawnMoreApples(lthing, -1, 2, 0, 1, 5, 1)
	--deactivateThing(lthing)
end

-- the bucket's small and can't hold very many
function bucket(lthing)
	spawnApples(1, addVectors(lthing.SpawnPosition, vector(0, 0.1, 0)))
	deactivateThing(lthing)
end

-- this on the other hand is huge
function containerLarge(lthing)
	spawnMoreApples(lthing, -1.14589, 2.02102, -1.11799, 4, 1, 4)
	deactivateThing(lthing)
end

function containerSmall(lthing)
	spawnMoreApples(lthing, -0.662419, 1.5571, -0.639155, 3, 1, 3)
	deactivateThing(lthing)
end
