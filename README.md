# Jusvibes
Imagine your world with a custom generative soundtrack: cozy vibes in a café, airy ambience in the kitchen, calm pads in the bedroom. This app analyzes your surroundings and generates the right music and visual feedback for the moment. Built with Unity and generative audio models.

# What inspired us

We were fascinated by the idea that a physical environment has its own hidden tone. Lighting, layout, texture, clutter, and openness all contribute to its emotional fingerprint. Spatial computing already blends digital content with the physical world, so we asked a simple question:
What would it feel like if your environment could compose its own soundtrack?

That idea pushed us to explore a system that combines scene understanding, generative audio, and immersive visual feedback.

# What we built

Jusvibes is an XR experience that turns your environment into music. You drop a small particle orb into your room, and the moment it lands Jusvibes performs a lightweight spatial scan. It captures images and picks up the overall vibe of the space. That environmental snapshot is sent to the server, where it’s transformed into a custom piece of music shaped by the room you’re standing in. The particle system then adapts to the music and ambience of the room, blending into your space.

# How we built it

We combined several technologies into a single pipeline:
-	Meta Quest spatial data for room mesh, movement tracking, and environmental context
-	Camera snapshots with vision models to classify the room’s mood
-	OpenAI API for image understanding and music prompt creation
-	Suno AI API for generative music creation
-	Spatial Sound for directional sound
-	Unity VFX Graph for real-time audio-reactive particles
-	A guided orb system that serves as an onboarding, scanning assistant, and visual anchor

Everything runs on Unity with a modular design so that each subsystem can evolve independently.

# Challenges we faced

- Creating a balance between transparency and a sense of magic for the user  
- Managing asynchronous AI processes without interrupting the experience  
- Dealing with microphone, camera, and XR permissions  
- Tuning the audio-reactive particle system to feel alive without becoming overwhelming  
- Working within the latency constraints of generative audio  

# What we didn't get time to implement
-	Voice to Speech to give additional prompting control to the user
- Scene Mesh with relighting so that particles reflect in your environment
- Onboarding 
- Afference ring haptic feedback
