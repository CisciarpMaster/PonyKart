-- set up our dictionaries of particles
-- our particle names will be "KartID", ex: "0" for the kart with ID 0
local particles = {}
local handlers = {}

local kartHandler = getKartHandler()

-- turns the particles on or off
local function emitting(kartID, isEmitting)
	particles[kartID].Emitting = isEmitting
end

-- creates the particles and attaches them to the kart
function pomfParticles_init(kart, id)
	local dirt = createParticleSystem("kartPomfParticle" .. id, "Pomf")
	
	-- turn off our particles so they don't emit for now
	dirt.Emitting = false
	
	-- and then attach it to the kart
	kart.RootNode:AttachObject(dirt)
	
	-- and put them in our dictionaries
	particles[id] = dirt
end

-- run this on every kart
forEachKart(pomfParticles_init)

local function onGround(kart, callback)
	emitting(kart.OwnerID, false)
	
	if handlers[kart.OwnerID] ~= nil then
		kartHandler.OnGround:Remove(handlers[kart.OwnerID])
		handlers[kart.OwnerID] = nil
	end
end

-- runs when we land
local function onTouchdown(kart, callback)
	emitting(kart.OwnerID, true)
	
	handlers[kart.OwnerID] = kartHandler.OnGround:Add(onGround)
end

-- then hook up to the events
kartHandler.OnTouchdown:Add(onTouchdown)
-- hopefully prevents a bug where sometimes the pomf particle would stay visible
kartHandler.OnCloseToTouchdown:Add(onGround)
kartHandler.OnLiftoff:Add(onGround)