using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace PlayablesPlugins
{
    public class LPSData
    {
        private ParticleSystem _particleSystem;
        private readonly Dictionary<string, PropertyInfo> _allProperties;
        
        public LPSData(ParticleSystem particleSystem)
        {
            _particleSystem = particleSystem;
            _allProperties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase)
            {
                // order of parameters: name|type|section
                // available types: float, int, color, bool, float_with_random, select_one, vector_3
                
                // main
                { "Duration|float|main", typeof(LPSData).GetProperty(nameof(Duration)) },
                { "Looping|bool|main", typeof(LPSData).GetProperty(nameof(Looping)) },
                { "Prewarm|bool|main", typeof(LPSData).GetProperty(nameof(Prewarm)) },
                { "Start Delay|float_with_random|main", typeof(LPSData).GetProperty(nameof(StartDelay)) },
                { "Start Lifetime|float_with_random|main", typeof(LPSData).GetProperty(nameof(StartLifetime)) },
                { "Start Speed|float_with_random|main", typeof(LPSData).GetProperty(nameof(StartSpeed)) },
                { "Start Size|float_with_random|main", typeof(LPSData).GetProperty(nameof(StartSize)) },
                { "Start Rotation|float_with_random|main", typeof(LPSData).GetProperty(nameof(StartRotation)) },
                //{ "Flip Rotation|float|main", typeof(LPSData).GetProperty(nameof(FlipRotation)) }, // compilation error
                { "Start Color|color|main", typeof(LPSData).GetProperty(nameof(StartColor)) },
                { "Gravity Modifier|float_with_random|main", typeof(LPSData).GetProperty(nameof(GravityModifier)) },
                { "Simulation Space|select_one|main", typeof(LPSData).GetProperty(nameof(SimulationSpace)) },
                { "Simulation Speed|float|main", typeof(LPSData).GetProperty(nameof(SimulationSpeed)) },
                { "Delta Time|select_one|main", typeof(LPSData).GetProperty(nameof(DeltaTime)) },
                { "Scaling Mode|select_one|main", typeof(LPSData).GetProperty(nameof(ScalingMode)) },
                { "Play On Awake|bool|main", typeof(LPSData).GetProperty(nameof(PlayOnAwake)) },
                //{ "Emitter Velocity Mode|select_one|main", typeof(LPSData).GetProperty(nameof(EmitterVelocityMode)) }, // compilation error
                { "Max Particles|int|main", typeof(LPSData).GetProperty(nameof(MaxParticles)) },
                //{ "Auto Random Seed|int|main", typeof(LPSData).GetProperty(nameof(AutoRandomSeed)) }, // how to address to this?
                //{ "Stop Action|select_one|main", typeof(LPSData).GetProperty(nameof(StopAction)) }, // compilation error
                //{ "Culling Mode|select_one|main", typeof(LPSData).GetProperty(nameof(CullingMode)) }, // compilation error
                //{ "Ring Buffer Mode|select_one|main", typeof(LPSData).GetProperty(nameof(RingBufferMode)) }, // compilation error
                
                // Emission
                { "Emission|bool|Emission", typeof(LPSData).GetProperty(nameof(Emission)) },
                { "Rate Over Time|float_with_random|Emission", typeof(LPSData).GetProperty(nameof(RateOverTime)) },
                { "Rate Over Distance|float_with_random|Emission", typeof(LPSData).GetProperty(nameof(RateOverDistance)) },
                
                // Shape
                { "Shape|bool|Shape", typeof(LPSData).GetProperty(nameof(Shape)) },
                { "Shape Figure|select_one|Shape", typeof(LPSData).GetProperty(nameof(ShapeFigure)) },
                { "Radius|float|Shape", typeof(LPSData).GetProperty(nameof(Radius)) },
                { "Radius Thickness|float|Shape", typeof(LPSData).GetProperty(nameof(RadiusThickness)) },
                { "Arc|float|Shape", typeof(LPSData).GetProperty(nameof(Arc)) },
                { "Position|vector_3|Shape", typeof(LPSData).GetProperty(nameof(Position)) },
                { "Rotation|vector_3|Shape", typeof(LPSData).GetProperty(nameof(Rotation)) },
                { "Scale|vector_3|Shape", typeof(LPSData).GetProperty(nameof(Scale)) },
                { "Align To Direction|bool|Shape", typeof(LPSData).GetProperty(nameof(AlignToDirection)) },
                { "Randomize Direction|float|Shape", typeof(LPSData).GetProperty(nameof(RandomizeDirection)) },
                { "Spherize Direction|float|Shape", typeof(LPSData).GetProperty(nameof(SpherizeDirection)) },
                { "Randomize Position|float|Shape", typeof(LPSData).GetProperty(nameof(RandomizePosition)) },
                
                // Limit Velocity over Lifetime
                { "Limit Velocity over Lifetime|bool|Limit Velocity over Lifetime", typeof(LPSData).GetProperty(nameof(LimitVelocityOverLifetime)) },
                { "Dampen|float|Limit Velocity over Lifetime", typeof(LPSData).GetProperty(nameof(Dampen)) },
                { "Drag|float_with_random|Limit Velocity over Lifetime", typeof(LPSData).GetProperty(nameof(Drag)) },
                { "MultiplyBySize|bool|Limit Velocity over Lifetime", typeof(LPSData).GetProperty(nameof(MultiplyBySize)) },
                { "MultiplyByVelocity|bool|Limit Velocity over Lifetime", typeof(LPSData).GetProperty(nameof(MultiplyByVelocity)) }, 
                
                // Inherit Velocity
                { "Inherit Velocity|bool|Inherit Velocity", typeof(LPSData).GetProperty(nameof(InheritVelocity)) },
                { "Mode|select_one|Inherit Velocity", typeof(LPSData).GetProperty(nameof(InheritVelocityMode)) },
                //{ "Multiply|float_with_random|Inherit Velocity", typeof(LPSData).GetProperty(nameof(MultiplyVelocityMode)) }, // how to address to this?

                // External Forces - compilation error for ALL

                // Texture Sheet Animation
                { "Texture Sheet Animation|bool|Texture Sheet Animation", typeof(LPSData).GetProperty(nameof(TextureSheetAnimation)) },
                { "Start Frame|float_with_random|Texture Sheet Animation", typeof(LPSData).GetProperty(nameof(StartFrame)) },
                { "Texture Sheet Animation Cycles|int|Texture Sheet Animation", typeof(LPSData).GetProperty(nameof(TextureSheetAnimationCycles)) },
                
                // Renderer
                { "Renderer|bool|Renderer", typeof(LPSData).GetProperty(nameof(Renderer)) },
                { "Render Mode|select_one|Renderer", typeof(LPSData).GetProperty(nameof(RenderModeParticles)) },
                { "Camera Scale (Stretched Billboard)|float|Renderer", typeof(LPSData).GetProperty(nameof(CameraScale)) },
                //{ "Length Scale|float|Renderer", typeof(LPSData).GetProperty(nameof(LengthScale)) }, // There is no compilation error, but in the build it gives undefined
                //{ "Freeform Stretching|float|Renderer", typeof(LPSData).GetProperty(nameof(FreeformStretching)) }, // compilation error
                //{ "Rotate with Stretch|float|Renderer", typeof(LPSData).GetProperty(nameof(RotateWithStretch)) }, // compilation error
                { "Normal Direction|float|Renderer", typeof(LPSData).GetProperty(nameof(NormalDirection)) },
                //{ "Sort Mode|select_one|Renderer", typeof(LPSData).GetProperty(nameof(SortMode)) }, // compilation error
                { "Sorting Fudge|float|Renderer", typeof(LPSData).GetProperty(nameof(SortingFudge)) },
                { "Min Particle Size|float|Renderer", typeof(LPSData).GetProperty(nameof(MinParticleSize)) },
                { "Max Particle Size|float|Renderer", typeof(LPSData).GetProperty(nameof(MaxParticleSize)) },
                { "Render Alignment|select_one|Renderer", typeof(LPSData).GetProperty(nameof(RenderAlignment)) },
                //{ "Flip|vector_3|Renderer", typeof(LPSData).GetProperty(nameof(Flip)) }, // compilation error
                //{ "Allow Roll|bool|Renderer", typeof(LPSData).GetProperty(nameof(AllowRoll)) }, // compilation error
                { "Pivot|vector_3|Renderer", typeof(LPSData).GetProperty(nameof(Pivot)) },
                //{ "Visualize Pivot|bool|Renderer", typeof(LPSData).GetProperty(nameof(VisualizePivot)) }, // how to address to this?
                //{ "Masking|select_one|Renderer", typeof(LPSData).GetProperty(nameof(Masking)) }, // compilation error
                //{ "Apply Active Color Space|bool|Renderer", typeof(LPSData).GetProperty(nameof(ApplyActiveColorSpace)) }, // how to address to this?
                //{ "Custom Vertex Streams|bool|Renderer", typeof(LPSData).GetProperty(nameof(CustomVertexStreams)) }, // how to address to this?
                { "Order In Layer (Sorting Layer)|int|Renderer", typeof(LPSData).GetProperty(nameof(OrderInLayer)) },
            };
        }

        #region main

        public float Duration
        {
            get => _particleSystem.main.duration;
            set
            {
                var main = _particleSystem.main;
                main.duration = value;
            }
        }
        
        public bool Looping
        {
            get => _particleSystem.main.loop;
            set
            {
                var main = _particleSystem.main;
                main.loop = value;
            }
        }
        
        public bool Prewarm
        {
            get => _particleSystem.main.prewarm;
            set
            {
                var main = _particleSystem.main;
                main.prewarm = value;
            }
        }
        
        public string StartDelay
        {
            get => GetterForPropertyRandomBetweenTwoConstants(_particleSystem.main.startDelay);
            set => SetterForPropertyRandomBetweenTwoConstants(
                value,
                x =>
                {
                    var main = _particleSystem.main;
                    main.startDelay = x;
                });
        }

        public string StartLifetime
        {
            get => GetterForPropertyRandomBetweenTwoConstants(_particleSystem.main.startLifetime);
            set => SetterForPropertyRandomBetweenTwoConstants(
                value,
                x =>
                {
                    var main = _particleSystem.main;
                    main.startLifetime = x;
                });
        }

        public string StartSpeed
        {
            get => GetterForPropertyRandomBetweenTwoConstants(_particleSystem.main.startSpeed);
            set => SetterForPropertyRandomBetweenTwoConstants(
                value,
                x =>
                {
                    var main = _particleSystem.main;
                    main.startSpeed = x;
                });
        }
        
        public string StartSize
        {
            get => GetterForPropertyRandomBetweenTwoConstants(_particleSystem.main.startSize);
            set => SetterForPropertyRandomBetweenTwoConstants(
                value,
                x =>
                {
                    var main = _particleSystem.main;
                    main.startSize = x;
                });
        }

        public string StartRotation
        {
            get => GetterForPropertyRandomBetweenTwoConstants(_particleSystem.main.startRotation);
            set => SetterForPropertyRandomBetweenTwoConstants(
                value,
                x =>
                {
                    var main = _particleSystem.main;
                    main.startRotation = x;
                });
        }

        public Color StartColor
        {
            get => _particleSystem.main.startColor.color;
            set
            {
                var main = _particleSystem.main;
                main.startColor = value;
            }
        }
        
        public string GravityModifier
        {
            get => GetterForPropertyRandomBetweenTwoConstants(_particleSystem.main.gravityModifier);
            set => SetterForPropertyRandomBetweenTwoConstants(
                value,
                x =>
                {
                    var main = _particleSystem.main;
                    main.gravityModifier = x;
                });
        }

        public string SimulationSpace
        {
            get
            {
                string answer = "";
                switch (_particleSystem.main.simulationSpace)
                {
                    case ParticleSystemSimulationSpace.Local:
                        answer = "local";
                        break;
                    case ParticleSystemSimulationSpace.World:
                        answer = "world";
                        break;
                    case ParticleSystemSimulationSpace.Custom:
                        answer = "custom";
                        break;
                    default:
                        answer = "local";
                        break;
                }
                
                return answer + "|local,world,custom";
            }
            set
            {
                var main = _particleSystem.main;
                if (string.Equals(value, "local", StringComparison.OrdinalIgnoreCase))
                {
                    main.simulationSpace = ParticleSystemSimulationSpace.Local;
                }
                else if (string.Equals(value, "world", StringComparison.OrdinalIgnoreCase))
                {
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                }
                else if (string.Equals(value, "custom", StringComparison.OrdinalIgnoreCase))
                {
                    main.simulationSpace = ParticleSystemSimulationSpace.Custom;
                }
            }
        }
        
        public float SimulationSpeed
        {
            get => _particleSystem.main.simulationSpeed;
            set
            {
                var main = _particleSystem.main;
                main.simulationSpeed = value;
            }
        }
        
        public string DeltaTime
        {
            get => (_particleSystem.main.useUnscaledTime ? "unscaled" : "scaled") + "|unscaled,scaled";
            set
            {
                var main = _particleSystem.main;
                if (string.Equals(value, "unscaled", StringComparison.OrdinalIgnoreCase))
                {
                    main.useUnscaledTime = true;
                }
                else if (string.Equals(value, "scaled", StringComparison.OrdinalIgnoreCase))
                {
                    main.useUnscaledTime = false;
                }
            }
        }
        
        public string ScalingMode
        {
            get
            {
                string answer = "";
                switch (_particleSystem.main.scalingMode)
                {
                    case ParticleSystemScalingMode.Hierarchy:
                        answer = "hierarchy";
                        break;
                    case ParticleSystemScalingMode.Local:
                        answer = "local";
                        break;
                    case ParticleSystemScalingMode.Shape:
                        answer = "shape";
                        break;
                    default:
                        answer = "hierarchy";
                        break;
                }
                
                return answer + "|hierarchy,local,shape";
            }
            set
            {
                var main = _particleSystem.main;
                if (string.Equals(value, "hierarchy", StringComparison.OrdinalIgnoreCase))
                {
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                }
                else if (string.Equals(value, "local", StringComparison.OrdinalIgnoreCase))
                {
                    main.scalingMode = ParticleSystemScalingMode.Local;
                }
                else if (string.Equals(value, "shape", StringComparison.OrdinalIgnoreCase))
                {
                    main.scalingMode = ParticleSystemScalingMode.Shape;
                }
            }
        }
        
        public bool PlayOnAwake
        {
            get => _particleSystem.main.playOnAwake;
            set
            {
                var main = _particleSystem.main;
                main.playOnAwake = value;
            }
        }
        
        public int MaxParticles
        {
            get => _particleSystem.main.maxParticles;
            set
            {
                var main = _particleSystem.main;
                main.maxParticles = value;
            }
        }
        
        #endregion
        
        #region Emission
        
        public bool Emission
        {
            get => _particleSystem.emission.enabled;
            set
            {
                var emission = _particleSystem.emission;
                emission.enabled = value;
            }
        }
        
        public string RateOverTime
        {
            get => GetterForPropertyRandomBetweenTwoConstants(_particleSystem.emission.rateOverTime);
            set => SetterForPropertyRandomBetweenTwoConstants(
                value,
                x =>
                {
                    var emission = _particleSystem.emission;
                    emission.rateOverTime = x;
                });
        }

        public string RateOverDistance
        {
            get => GetterForPropertyRandomBetweenTwoConstants(_particleSystem.emission.rateOverDistance);
            set => SetterForPropertyRandomBetweenTwoConstants(
                value,
                x =>
                {
                    var emission = _particleSystem.emission;
                    emission.rateOverDistance = x;
                });
        }

        #endregion

        #region Shape

        public bool Shape
        {
            get => _particleSystem.shape.enabled;
            set
            {
                var shape = _particleSystem.shape;
                shape.enabled = value;
            }
        }

        public string ShapeFigure
        {
            get
            {
                string answer = "";
                switch (_particleSystem.shape.shapeType)
                {
                    case ParticleSystemShapeType.Sphere:
                        answer = "sphere";
                        break;
                    case ParticleSystemShapeType.Hemisphere:
                        answer = "hemisphere";
                        break;
                    case ParticleSystemShapeType.Cone:
                        answer = "cone";
                        break;
                    case ParticleSystemShapeType.Donut:
                        answer = "donut";
                        break;
                    case ParticleSystemShapeType.Box:
                        answer = "box";
                        break;
                    /*
                    case ParticleSystemShapeType.Mesh:
                        answer = "mesh";
                        break;
                    case ParticleSystemShapeType.MeshRenderer:
                        answer = "mesh_renderer";
                        break;
                    case ParticleSystemShapeType.SkinnedMeshRenderer:
                        answer = "skinned_mesh_renderer";
                        break;
                    case ParticleSystemShapeType.Sprite:
                        answer = "sprite";
                        break;
                    case ParticleSystemShapeType.SpriteRenderer:
                        answer = "sprite_renderer";
                        break;
                    */
                    case ParticleSystemShapeType.Circle:
                        answer = "circle";
                        break;
                    case ParticleSystemShapeType.BoxEdge:
                        answer = "box_edge";
                        break;
                    case ParticleSystemShapeType.BoxShell:
                        answer = "box_shell";
                        break;
                    case ParticleSystemShapeType.SingleSidedEdge:
                        answer = "single_sided_edge";
                        break;
                    case ParticleSystemShapeType.Rectangle:
                        answer = "rectangle";
                        break;
                    default:
                        answer = "unsupported_figure_type";
                        break;
                }
                
                return answer + "|sphere,hemisphere,cone,donut,box," +
                       // "mesh,mesh_renderer,skinned_mesh_renderer,sprite,sprite_renderer," + // compilation error
                       "circle,box_edge,rectangle";
            }
            set
            {
                if (string.Equals(value, "unsupported_figure_type", StringComparison.OrdinalIgnoreCase))
                    return;
                
                var shape = _particleSystem.shape;
                if (string.Equals(value, "sphere", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                }
                else if (string.Equals(value, "hemisphere", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.Hemisphere;
                }
                else if (string.Equals(value, "cone", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.Cone;
                }
                else if (string.Equals(value, "donut", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.Donut;
                }
                else if (string.Equals(value, "box", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.Box;
                }
                /* compilation error
                else if (string.Equals(value, "mesh", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.Mesh;
                }
                else if (string.Equals(value, "mesh_renderer", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.MeshRenderer;
                }
                else if (string.Equals(value, "skinned_mesh_renderer", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                }
                else if (string.Equals(value, "sprite", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.Sprite;
                }
                else if (string.Equals(value, "sprite_renderer", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.SpriteRenderer;
                }
                */
                else if (string.Equals(value, "circle", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.Circle;
                }
                else if (string.Equals(value, "box_edge", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.BoxEdge;
                }
                else if (string.Equals(value, "box_shell", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.BoxShell;
                }
                else if (string.Equals(value, "single_sided_edge", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
                }
                else if (string.Equals(value, "rectangle", StringComparison.OrdinalIgnoreCase))
                {
                    shape.shapeType = ParticleSystemShapeType.Rectangle;
                }
            }
        }
        
        public float Radius
        {
            get => _particleSystem.shape.radius;
            set
            {
                var shape = _particleSystem.shape;
                shape.radius = value;
            }
        }
        
        public float RadiusThickness
        {
            get => _particleSystem.shape.radiusThickness;
            set
            {
                var shape = _particleSystem.shape;
                shape.radiusThickness = value;
            }
        }

        public float Arc
        {
            get => _particleSystem.shape.arc;
            set
            {
                var shape = _particleSystem.shape;
                shape.arc = value;
            }
        }
        
        public string Position
        {
            get => _particleSystem.shape.position.x
                   + "|" + _particleSystem.shape.position.y
                   + "|" + _particleSystem.shape.position.z;
            set
            {
                var shape = _particleSystem.shape;
                shape.position = new Vector3(
                    float.Parse(value.Split('|')[0], CultureInfo.InvariantCulture), 
                    float.Parse(value.Split('|')[1], CultureInfo.InvariantCulture), 
                    float.Parse(value.Split('|')[2], CultureInfo.InvariantCulture));
            }
        }

        public string Rotation
        {
            get => _particleSystem.shape.rotation.x
                   + "|" + _particleSystem.shape.rotation.y
                   + "|" + _particleSystem.shape.rotation.z;
            set
            {
                var shape = _particleSystem.shape;
                shape.rotation = new Vector3(
                    float.Parse(value.Split('|')[0], CultureInfo.InvariantCulture), 
                    float.Parse(value.Split('|')[1], CultureInfo.InvariantCulture), 
                    float.Parse(value.Split('|')[2], CultureInfo.InvariantCulture));
            }
        }

        public string Scale
        {
            get => _particleSystem.shape.scale.x
                   + "|" + _particleSystem.shape.scale.y
                   + "|" + _particleSystem.shape.scale.z;
            set
            {
                var shape = _particleSystem.shape;
                shape.scale = new Vector3(
                    float.Parse(value.Split('|')[0], CultureInfo.InvariantCulture), 
                    float.Parse(value.Split('|')[1], CultureInfo.InvariantCulture), 
                    float.Parse(value.Split('|')[2], CultureInfo.InvariantCulture));
            }
        }

        public bool AlignToDirection
        {
            get => _particleSystem.shape.alignToDirection;
            set
            {
                var shape = _particleSystem.shape;
                shape.alignToDirection = value;
            }
        }
        
        public float RandomizeDirection
        {
            get => _particleSystem.shape.randomDirectionAmount;
            set
            {
                var shape = _particleSystem.shape;
                shape.randomDirectionAmount = value;
            }
        }
        
        public float SpherizeDirection
        {
            get => _particleSystem.shape.sphericalDirectionAmount;
            set
            {
                var shape = _particleSystem.shape;
                shape.sphericalDirectionAmount = value;
            }
        }
        
        public float RandomizePosition
        {
            get => _particleSystem.shape.randomPositionAmount;
            set
            {
                var shape = _particleSystem.shape;
                shape.randomPositionAmount = value;
            }
        }
        
        #endregion
        
        #region Limit Velocity over Lifetime

        public bool LimitVelocityOverLifetime
        {
            get => _particleSystem.limitVelocityOverLifetime.enabled;
            set
            {
                var limit = _particleSystem.limitVelocityOverLifetime;
                limit.enabled = value;
            }
        }
        
        public float Dampen
        {
            get => _particleSystem.limitVelocityOverLifetime.dampen;
            set
            {
                var main = _particleSystem.limitVelocityOverLifetime;
                main.dampen = value;
            }
        }
        
        public string Drag
        {
            get => GetterForPropertyRandomBetweenTwoConstants(_particleSystem.limitVelocityOverLifetime.drag);
            set => SetterForPropertyRandomBetweenTwoConstants(
                value,
                x =>
                {
                    var limitVelocityOverLifetime = _particleSystem.limitVelocityOverLifetime;
                    limitVelocityOverLifetime.drag = x;
                });
        }

        public bool MultiplyBySize
        {
            get => _particleSystem.limitVelocityOverLifetime.multiplyDragByParticleSize;
            set
            {
                var limit = _particleSystem.limitVelocityOverLifetime;
                limit.multiplyDragByParticleSize = value;
            }
        }
        
        public bool MultiplyByVelocity
        {
            get => _particleSystem.limitVelocityOverLifetime.multiplyDragByParticleVelocity;
            set
            {
                var limit = _particleSystem.limitVelocityOverLifetime;
                limit.multiplyDragByParticleVelocity = value;
            }
        }
        
        #endregion

        #region Inherit Velocity

        public bool InheritVelocity
        {
            get => _particleSystem.inheritVelocity.enabled;
            set
            {
                var inherit = _particleSystem.inheritVelocity;
                inherit.enabled = value;
            }
        }

        public string InheritVelocityMode
        {
            get
            {
                string answer = "";
                switch (_particleSystem.inheritVelocity.mode)
                {
                    case ParticleSystemInheritVelocityMode.Initial:
                        answer = "initial";
                        break;
                    case ParticleSystemInheritVelocityMode.Current:
                        answer = "current";
                        break;
                    default:
                        answer = "initial";
                        break;
                }
                
                return answer + "|initial,current";
            }
            set
            {
                var inherit = _particleSystem.inheritVelocity;
                if (string.Equals(value, "initial", StringComparison.OrdinalIgnoreCase))
                {
                    inherit.mode = ParticleSystemInheritVelocityMode.Initial;
                }
                else if (string.Equals(value, "current", StringComparison.OrdinalIgnoreCase))
                {
                    inherit.mode = ParticleSystemInheritVelocityMode.Current;
                }
            }
        }
        
        #endregion

        #region Texture Sheet Animation

        public bool TextureSheetAnimation
        {
            get => _particleSystem.textureSheetAnimation.enabled;
            set
            {
                var textureSheetAnimation = _particleSystem.textureSheetAnimation;
                textureSheetAnimation.enabled = value;
            }
        }
        
        public string StartFrame
        {
            get => GetterForPropertyRandomBetweenTwoConstants(_particleSystem.textureSheetAnimation.startFrame);
            set => SetterForPropertyRandomBetweenTwoConstants(
                value,
                x =>
                {
                    var textureSheetAnimation = _particleSystem.textureSheetAnimation;
                    textureSheetAnimation.startFrame = x;
                });
        }
        
        public int TextureSheetAnimationCycles
        {
            get => _particleSystem.textureSheetAnimation.cycleCount;
            set
            {
                var textureSheetAnimation = _particleSystem.textureSheetAnimation;
                textureSheetAnimation.cycleCount = value;
            }
        }

        #endregion
        
        #region Renderer
        
        public bool Renderer
        {
            get => _particleSystem.GetComponent<ParticleSystemRenderer>().enabled;
            set
            {
                var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.enabled = value;
            }
        }

        public string RenderModeParticles
        {
            get
            {
                string answer = "";
                switch (_particleSystem.GetComponent<ParticleSystemRenderer>().renderMode)
                {
                    case ParticleSystemRenderMode.Billboard:
                        answer = "billboard";
                        break;
                    case ParticleSystemRenderMode.Stretch:
                        answer = "stretched_billboard";
                        break;
                    case ParticleSystemRenderMode.HorizontalBillboard:
                        answer = "horizontal_billboard";
                        break;
                    case ParticleSystemRenderMode.VerticalBillboard:
                        answer = "vertical_billboard";
                        break;
                    case ParticleSystemRenderMode.Mesh:
                        answer = "mesh";
                        break;
                    default:
                        answer = "none";
                        break;
                }
                
                return answer + "|billboard,stretched_billboard,horizontal_billboard,vertical_billboard,mesh,none";
            }
            set
            {
                var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                if (string.Equals(value, "billboard", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                }
                else if (string.Equals(value, "stretched_billboard", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.renderMode = ParticleSystemRenderMode.Stretch;
                }
                else if (string.Equals(value, "horizontal_billboard", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                }
                else if (string.Equals(value, "vertical_billboard", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.renderMode = ParticleSystemRenderMode.VerticalBillboard;
                }
                else if (string.Equals(value, "mesh", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.renderMode = ParticleSystemRenderMode.Mesh;
                }
                else if (string.Equals(value, "none", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.renderMode = ParticleSystemRenderMode.None;
                }
            }
        }
        
        public float CameraScale
        {
            get => _particleSystem.GetComponent<ParticleSystemRenderer>().cameraVelocityScale;
            set
            {
                var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.cameraVelocityScale = value;
            }
        }

        public float NormalDirection
        {
            get => _particleSystem.GetComponent<ParticleSystemRenderer>().normalDirection;
            set
            {
                var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.normalDirection = value;
            }
        }

        public float SortingFudge
        {
            get => _particleSystem.GetComponent<ParticleSystemRenderer>().sortingFudge;
            set
            {
                var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.sortingFudge = value;
            }
        }
        
        public float MinParticleSize
        {
            get => _particleSystem.GetComponent<ParticleSystemRenderer>().minParticleSize;
            set
            {
                var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.minParticleSize = value;
            }
        }
        
        public float MaxParticleSize
        {
            get => _particleSystem.GetComponent<ParticleSystemRenderer>().maxParticleSize;
            set
            {
                var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.maxParticleSize = value;
            }
        }
        
        public string RenderAlignment
        {
            get
            {
                string answer = "";
                switch (_particleSystem.GetComponent<ParticleSystemRenderer>().alignment)
                {
                    case ParticleSystemRenderSpace.View:
                        answer = "view";
                        break;
                    case ParticleSystemRenderSpace.World:
                        answer = "world";
                        break;
                    case ParticleSystemRenderSpace.Local:
                        answer = "local";
                        break;
                    case ParticleSystemRenderSpace.Facing:
                        answer = "facing";
                        break;
                    case ParticleSystemRenderSpace.Velocity:
                        answer = "velocity";
                        break;
                    default:
                        answer = "view";
                        break;
                }
                
                return answer + "|view,world,local,facing,velocity";
            }
            set
            {
                var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                if (string.Equals(value, "view", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.alignment = ParticleSystemRenderSpace.View;
                }
                else if (string.Equals(value, "world", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.alignment = ParticleSystemRenderSpace.World;
                }
                else if (string.Equals(value, "local", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.alignment = ParticleSystemRenderSpace.Local;
                }
                else if (string.Equals(value, "facing", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.alignment = ParticleSystemRenderSpace.Facing;
                }
                else if (string.Equals(value, "velocity", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.alignment = ParticleSystemRenderSpace.Velocity;
                }
            }
        }
        
        public string Pivot
        {
            get => _particleSystem.GetComponent<ParticleSystemRenderer>().pivot.x
                   + "|" + _particleSystem.GetComponent<ParticleSystemRenderer>().pivot.y
                   + "|" + _particleSystem.GetComponent<ParticleSystemRenderer>().pivot.z;
            set
            {
                var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.pivot = new Vector3(
                    float.Parse(value.Split('|')[0], CultureInfo.InvariantCulture), 
                    float.Parse(value.Split('|')[1], CultureInfo.InvariantCulture), 
                    float.Parse(value.Split('|')[2], CultureInfo.InvariantCulture));
            }
        }

        public int OrderInLayer
        {
            get => _particleSystem.GetComponent<ParticleSystemRenderer>().sortingOrder;
            set
            {
                var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.sortingOrder = value;
            }
        }

        #endregion

        #region RandomBetweenTwoConstants

        private string GetterForPropertyRandomBetweenTwoConstants(ParticleSystem.MinMaxCurve property)
        {
            if (property.mode == ParticleSystemCurveMode.Constant)
                return property.constant.ToString(CultureInfo.InvariantCulture);

            if (property.mode == ParticleSystemCurveMode.TwoConstants)
            {
                string minStr = property.constantMin.ToString(CultureInfo.InvariantCulture);
                string maxStr = property.constantMax.ToString(CultureInfo.InvariantCulture);
                return minStr + "|" + maxStr;
            }

            if (property.mode == ParticleSystemCurveMode.Curve)
                return "curve";

            return "two_curves";
        }

        private void SetterForPropertyRandomBetweenTwoConstants(
            string value,
            Action<ParticleSystem.MinMaxCurve> apply)
        {
            if (value.Contains("curve"))
                return;

            ParticleSystem.MinMaxCurve curve;

            if (value.Contains("|"))
            {
                curve = new ParticleSystem.MinMaxCurve(
                    float.Parse(value.Split('|')[0], CultureInfo.InvariantCulture),
                    float.Parse(value.Split('|')[1], CultureInfo.InvariantCulture));
            }
            else
            {
                curve = new ParticleSystem.MinMaxCurve(
                    float.Parse(value, CultureInfo.InvariantCulture));
            }

            apply(curve);
        }
        
        #endregion

        public string GetData()
        {
            if (_particleSystem == null)
            {
                Debug.LogError("[LPSData Unity] ParticleSystem is null. Cannot read data.");
                return "{}";
            }

            var builder = new StringBuilder("{");
            var isFirst = true;

            foreach (var pair in _allProperties)
            {
                var property = pair.Value;
                if (property == null || !property.CanRead)
                {
                    continue;
                }

                var rawValue = property.GetValue(this, null);
                if (!DataParsing.TrySerializeValueByDeclaredType(pair.Key, rawValue, out var serializedValue))
                {
                    continue;
                }

                if (!isFirst)
                {
                    builder.Append(", ");
                }

                builder.Append('"').Append(pair.Key).Append("\": ")
                    .Append(serializedValue);
                isFirst = false;
            }

            builder.Append('}');
            return builder.ToString();
        }

        public void SetData(string data, int id, bool logsEnabled
#if UNITY_EDITOR
            , bool isThisEditorSet = false  
#endif
        )
        {
            if (logsEnabled)
                Debug.Log($"[LPSData Unity] Get data: {data}");
            
            if (_particleSystem == null)
            {
                Debug.LogError("[LPSData Unity] ParticleSystem is null. Cannot apply data.");
                return;
            }

#if UNITY_EDITOR
            var ignoreId = isThisEditorSet;
            
            if (isThisEditorSet)
                Debug.Log($"[LPSData Unity] Set dev data: {data}");
#else
            const bool ignoreId = false;
#endif

            var settingsJson = DataParsing.ExtractSettingsJson(data, id, ignoreId);
            if (string.IsNullOrEmpty(settingsJson))
            {
                return;
            }

            var values = DataParsing.ParseFlatObject(settingsJson);
            if (values.Count == 0)
            {
                Debug.LogWarning($"[LPSData Unity] Settings payload for id '{id}' is empty.");
                return;
            }

            foreach (var pair in values)
            {
                if (!DataParsing.TryGetProperty(_allProperties, pair.Key, out var property, out var resolvedKey) || property == null)
                {
                    Debug.LogWarning($"[LPSData Unity] Unknown field '{pair.Key}' - skipped.");
                    continue;
                }

                if (!property.CanWrite)
                {
                    Debug.LogWarning($"[LPSData Unity] Field '{pair.Key}' is read-only - skipped.");
                    continue;
                }

                if (!DataParsing.TryConvertByDeclaredType(resolvedKey, pair.Value, out var convertedValue))
                {
                    Debug.LogError($"[LPSData Unity] Cannot convert '{pair.Key}' value '{pair.Value}' for declared type '{DataParsing.GetSettingType(resolvedKey)}'.");
                    continue;
                }

                property.SetValue(this, convertedValue, null);
                
                if (logsEnabled)
                    Debug.Log($"[LPSData Unity] Applied {resolvedKey} = {convertedValue}");
            }
        }
    }
}