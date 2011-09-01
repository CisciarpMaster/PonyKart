-- basically since all of the fences are pointing "outwards" from the track, this rotates the collision shape so the shape is pointing "inwards"
-- this makes it harder to try driving up the walls
function fence(lthing)
	quat = quaternionFromAngleAxis(3.14159, vector(0, 1, 0))
	quat = multiplyQuaternion(quat, lthing.Body.Orientation)
	setBodyOrientation(lthing.Body, quat)
end