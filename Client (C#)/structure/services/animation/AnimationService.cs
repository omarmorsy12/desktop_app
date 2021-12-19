using app.structure.animations;

namespace app.structure.services
{
    class AnimationService : Services
    {
        public readonly MovementAnimation movement = new MovementAnimation();
        public readonly OpacityAnimation opacity = new OpacityAnimation();
        public readonly WidthAnimation width = new WidthAnimation();
        public readonly HeightAnimation height = new HeightAnimation();
    }
}
