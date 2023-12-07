/*
 * [ Sonic Onset Adventure]
 * Copyright (c) 2023 Regan "CKDEV" Green
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using Godot;
using System.Collections.Generic;

namespace SonicGodot.Character.Sonic
{
    public partial class ModelRoot : Character.ModelRoot
    {
        // Model nodes
        private MeshInstance3D _jumpball_node;

        // Tilt state
        private TiltBone _tiltboneHead;
        private TiltBone _tiltboneUpperTorso;
        private TiltBone _tiltboneLowerTorso;

        private RootBone _rootbone;

        private Util.FixedSlerp _headPointOfInterest = new Util.FixedSlerp(Quaternion.Identity);
        private Util.FixedSlerp _upperTorsoPointOfInterest = new Util.FixedSlerp(Quaternion.Identity);

        // Tilt quaternions
        private Quaternion _headTilt = Quaternion.Identity;
        private Quaternion _upperTorsoTilt = Quaternion.Identity;
        private Quaternion _lowerTorsoTilt = Quaternion.Identity;

        // Animation functions
        public override void PlayAnimation(string name, double speed = 1.0f)
        {
            // Play root animation
            base.PlayAnimation(name, speed);

            // Get spinning opacity
            float opacity = 0.0f;
            if (name == "Spin" || name == "RollFloor")
            {
                opacity = Mathf.Clamp(Mathf.Abs((float)speed) * 0.3f - 0.8f, 0.0f, 1.0f);
            }

            // Flicker spindash every frame, but in 16 frame intervals
            // if (aclass == "Spindash" && (Root.GetClock() & 0x11) == 0)
            // 	opacity = 0.0f;

            // Set jumpball opacity
            _jumpball_node.Transparency = 1.0f - opacity;
        }

        // Model root node
        public override void _Ready()
        {
            // Get nodes
            _animationPlayer = GetNode<Animator>("Model/AnimationPlayer");
            _skeletonNode = GetNode<Skeleton3D>("Model/Armature/Skeleton3D");

            _jumpball_node = GetNode<MeshInstance3D>("JumpballRoot/Jumpball/Jumpball");

            // Setup tilt bones
            _tiltboneHead = new TiltBone(_skeletonNode, "Head");
            _tiltboneUpperTorso = new TiltBone(_skeletonNode, "Spine1");
            _tiltboneLowerTorso = new TiltBone(_skeletonNode, "Hips");

            _rootbone = new RootBone(_skeletonNode, "ROOT");

            // Get animation tracks
            _restPose = new Util.Animation.SkeletonPose(_skeletonNode);
            foreach (string animation in _animationPlayer.GetAnimationList())
            {
                _animationTracks[animation] = new Util.Animation.AnimationTrack(_skeletonNode, _animationPlayer.GetAnimation(animation));
            }

            // Add animations
            _animationPlayer.m_specs["Idle"] = new Animator.AnimationSpec(0.08, false, false, null,
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Idle") },
                new Dictionary<string, double>() { { "Run", 0.3 } }
            );


            _animationPlayer.m_specs["Run"] = new Animator.AnimationSpec(0.08, false, false, null, new Animator.AnimationSpec.TrackSpec[]
                {
                    new Animator.AnimationSpec.TrackSpec("Walk", 0.0, 0.0, 0.2, 0.5),
                    new Animator.AnimationSpec.TrackSpec("Jog", 2.3, 1.0, 0.2, 0.3),
                    new Animator.AnimationSpec.TrackSpec("Run", 3.7, 0.4, 0.2, 0.5),
                    new Animator.AnimationSpec.TrackSpec("Run", 5.09, 1.5, 0.2, 0.9),
                    new Animator.AnimationSpec.TrackSpec("MachRun", 7.635, 4.0, 0.2, 0.9),
                },
                new Dictionary<string, double>() { { "Idle", 0.2 } }
            );

            _animationPlayer.m_specs["Brake"] = new Animator.AnimationSpec(0.08, false, false, null,
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Brake") }
            );
            _animationPlayer.m_specs["Up"] = new Animator.AnimationSpec(0.08, false, false, null,
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Up") },
                new Dictionary<string, double>() { { "Fall", 0.5 } }
            );

            _animationPlayer.m_specs["Spin"] = new Animator.AnimationSpec(0.05, false, false, null,
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Spin") }
            );
            _animationPlayer.m_specs["Spin_charge"] = new Animator.AnimationSpec(0.05, false, false, null,
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Spin_charge") }
            );

            _animationPlayer.m_specs["Hurt"] = new Animator.AnimationSpec(0.13, false, false, null,
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Hurt") }
            );
            _animationPlayer.m_specs["Dead"] = new Animator.AnimationSpec(0.25, false, false, null,
               new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Dead") }
           );
            _animationPlayer.m_specs["Fall"] = new Animator.AnimationSpec(0.13, false, false, null,
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Fall") }
            );
            _animationPlayer.m_specs["FallFast"] = new Animator.AnimationSpec(0.13, false, false, null,
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("FallFast") }
            );
            _animationPlayer.m_specs["Forward"] = new Animator.AnimationSpec(0.13, false, false, null,
    new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Forward") }
);
            _animationPlayer.m_specs["Land"] = new Animator.AnimationSpec(0.08, false, false, "Idle",
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Land") }
            );

            _animationPlayer.m_specs["Jump"] = new Animator.AnimationSpec(0.08, false, false, "Fall",
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Jump") }
            );
            _animationPlayer.m_specs["KickAir"] = new Animator.AnimationSpec(0.08, false, false, "Fall",
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("KickAir") }
            );
            _animationPlayer.m_specs["HomingTrick"] = new Animator.AnimationSpec(0.08, false, false, "Fall",
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("HomingTrick") }
            );
            _animationPlayer.m_specs["HomingTrick - Japan"] = new Animator.AnimationSpec(0.08, false, false, "Fall",
                new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("HomingTrick - Japan") }
            );
            // Setup base
            base._Ready();
        }

        public override void _PhysicsProcess(double delta)
        {
            // Process base
            base._PhysicsProcess(delta);

            // Update point of interest angles
            float point_of_interest_angle_y = 0.0f;
            float point_of_interest_angle_x = 0.0f;

            if (_pointOfInterest.HasValue)
            {
                // Get point of interest relative to neck
                Vector3 pointOfInterest = _pointOfInterest.Value;
                Transform3D neckTransform = GlobalTransform * _skeletonNode.GetBoneGlobalRest(_tiltboneHead._boneIdx);
                Vector3 pointOfInterestFromNeck = neckTransform.Inverse() * pointOfInterest;

                // Get point of interest angles
                point_of_interest_angle_y = Mathf.Atan2(-pointOfInterestFromNeck.X, -pointOfInterestFromNeck.Z);
                point_of_interest_angle_x = Mathf.Atan2(-pointOfInterestFromNeck.Y * 2.0f, Mathf.Sqrt(pointOfInterestFromNeck.X * pointOfInterestFromNeck.X + pointOfInterestFromNeck.Z * pointOfInterestFromNeck.Z));

                point_of_interest_angle_y = Mathf.Clamp(point_of_interest_angle_y, Mathf.Pi * -0.5f, Mathf.Pi * 0.5f);
                point_of_interest_angle_x = Mathf.Clamp(point_of_interest_angle_x, Mathf.Pi * -0.2f, Mathf.Pi * 0.1f);
            }

            // Get point of interest quaternions
            Quaternion upperTorsoPointOfInterest = new Quaternion(new Vector3(0.0f, 1.0f, 0.0f), point_of_interest_angle_y * 0.3f);
            upperTorsoPointOfInterest *= new Quaternion(new Vector3(1.0f, 0.0f, 0.0f), point_of_interest_angle_x * 0.4f);

            Quaternion headPointOfInterest = new Quaternion(new Vector3(0.0f, 1.0f, 0.0f), point_of_interest_angle_y * 0.4f);
            headPointOfInterest *= new Quaternion(new Vector3(1.0f, 0.0f, 0.0f), point_of_interest_angle_x * 0.5f);

            // Interpolate quaternions
            _headPointOfInterest.Step(headPointOfInterest, 0.2f);
            _upperTorsoPointOfInterest.Step(upperTorsoPointOfInterest, 0.2f);
        }

        public override void _Process(double delta)
        {
            // Process base
            base._Process(delta);
            // Get interpolation fraction
            float fraction = (float)Engine.GetPhysicsInterpolationFraction();

            // Shear root bone
            _rootbone.Shear(_shear);

            // Tilt bones
            _headTilt = new Quaternion(new Vector3(0.0f, 1.0f, 0.0f), _tilt.Pos * 0.15f);
            _headTilt *= new Quaternion(new Vector3(0.0f, 0.0f, 1.0f), _tilt.Pos * 0.1f);
            _headTilt *= new Quaternion(new Vector3(1.0f, 0.0f, 0.0f), -Mathf.Abs(_tilt.Pos * 0.3f));

            _upperTorsoTilt = new Quaternion(new Vector3(0.0f, 0.0f, 1.0f), _tilt.Pos * -0.3f);
            _upperTorsoTilt *= new Quaternion(new Vector3(0.0f, 1.0f, 0.0f), _tilt.Pos * 0.3f);

            _lowerTorsoTilt = new Quaternion(new Vector3(0.0f, 0.0f, 1.0f), _tilt.Pos * -0.25f);

            // Apply point of interest quaternions
            _headTilt *= _headPointOfInterest.Get(fraction);
            _upperTorsoTilt *= _upperTorsoPointOfInterest.Get(fraction);

            // Tilt bones
            _tiltboneHead.Tilt(_headTilt);
            _tiltboneUpperTorso.Tilt(_upperTorsoTilt);
            _tiltboneLowerTorso.Tilt(_lowerTorsoTilt);
        }

    }
}
