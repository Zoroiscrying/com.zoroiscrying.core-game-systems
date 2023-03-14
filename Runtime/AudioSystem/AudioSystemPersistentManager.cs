using System;
using System.Collections;
using UnityEngine;
using Zoroiscrying.CoreGameSystems.AudioSystem.ScriptableObjectIntegration;
using Zoroiscrying.CoreGameSystems.CoreSystemBase;

namespace Zoroiscrying.CoreGameSystems.AudioSystem
{
    public class AudioSystemPersistentManager : BasePersistentManager<AudioSystemPersistentManager>
    {
        [SerializeField] private AudioSource audioSourcePoolPrefab;
        [SerializeField] private Transform audioSourcePoolParent;
        private AudioSourcePool audioSourcePool;
        [SerializeField] private PlayAudioEventSo playAudioEvent;
        
        //relationships between audio object names(key) and audio tracks(value) (Audio Tracks)
        //private Hashtable m_AudioTable;
        //relationships between audio objects(key) and jobs(value) (Coroutine, IEnumerator)
        private Hashtable _jobTable;
        //different audio tracks
        [SerializeField] private AudioTrackSo[] audioTracks;
        
        public enum AudioJobAction
        {
            START,
            STOP,
            RESTART
        }
        
        private class AudioJob
        {
            public AudioJobAction Action;
            public AudioObjectSo AudioObject;
            public bool Fade;
            public float FadeTime;
            public float DelayTime;

            public AudioJob(AudioJobAction action, AudioObjectSo audioObject, bool fade, float fadeTime, float delayTime)
            {
                this.Action = action;
                this.AudioObject = audioObject;
                this.Fade = fade;
                this.FadeTime = fadeTime;
                this.DelayTime = delayTime;
            }
        }

        #region Unity Functions

        private void OnDisable()
        {
            Dispose();
        }

        #endregion
        
        #region Public Functions

        public void PlayAudio(AudioObjectSo audioObject, bool fade = false, float fadeTime = 0.2f, float delay = 0.0f)
        {
            AddJob(new AudioJob(AudioJobAction.START, audioObject, fade, fadeTime, delay));
        }

        public void StopAudio(AudioObjectSo audioObject, bool fade = false, float fadeTime = 0.2f, float delay = 0.0f)
        {
            AddJob(new AudioJob(AudioJobAction.STOP, audioObject, fade, fadeTime, delay));
        }

        public void RestartAudio(AudioObjectSo audioObject, bool fade = false, float fadeTime = 0.2f, float delay = 0.0f)
        {
            AddJob(new AudioJob(AudioJobAction.RESTART, audioObject, fade, fadeTime, delay));
        }

        #endregion

        #region Private Functions
        
        private void PlayAudio(PlayAudioSetting setting)
        {
            AddJob(new AudioJob(setting.audioAction, setting.audioObject, setting.fade, setting.fadeTime,
                setting.delay));
        }
        
        private void AddJob(AudioJob audioJob)
        {
            //remove conflicting jobs if the track is not a multi-playing track
            var audioObjectTrack = audioJob.AudioObject.PlayingTrack;
            
            // If the track is not a multi-playing track,
            // the playing audio that shares the audio track with this one should be turned off
            if (!audioObjectTrack.MultiPlayingTrack)
            {
                // If the action is not stop, the audios in the track should be turned off.
                if (audioJob.Action == AudioJobAction.START || audioJob.Action == AudioJobAction.RESTART)
                {
                    if (audioObjectTrack.PlayingObject == audioJob.AudioObject && audioObjectTrack.IsPlaying())
                    {
                        Debug.Log(audioObjectTrack.PlayingObject.name + "||" + audioJob.AudioObject.name);
                        return;
                    }
                    RemoveConflictingJobs(audioJob.AudioObject);
                }
                // If the audio to stop is not currently playing, then there is no need to stop the action.
                else if (audioJob.Action == AudioJobAction.STOP)
                {
                    if (audioObjectTrack.PlayingObject != audioJob.AudioObject || !audioObjectTrack.IsPlaying())
                    {
                        return;
                    }
                }
            }
            
            //start the job
            IEnumerator jobRunner = RunAudioJob(audioJob);
            if (!audioObjectTrack.MultiPlayingTrack)
            {
                _jobTable.Add(audioJob.AudioObject, jobRunner);   
            }
            StartCoroutine(jobRunner);
            //Debug.Log("Starting job on Audio Object[" + audioJob.AudioObject.name + "] with operation: " + audioJob.Action);
        }
        
        private void RemoveConflictingJobs(AudioObjectSo audioObject)
        {
            // check if the audio object shares the same track with the running jobs
            // and kills the conflicting job if it is.
            AudioObjectSo conflictingAudioObject = null;
            foreach (DictionaryEntry entry in _jobTable)
            {
                AudioObjectSo aO = (AudioObjectSo)entry.Key;
                AudioTrackSo audioTrackInUse = aO.PlayingTrack;
                AudioTrackSo audioTrackNeeded = audioObject.PlayingTrack;
                if (audioTrackNeeded == audioTrackInUse)
                {
                    //there is a conflict, we store the type of it and remove the job accordingly
                    conflictingAudioObject = aO;
                }
            }

            if (conflictingAudioObject != null)
            {
                RemoveJob(conflictingAudioObject);
            }
        }
        
        private void RemoveJob(AudioObjectSo audioObject)
        {
            if (!_jobTable.ContainsKey(audioObject))
            { 
                Debug.LogWarning("Trying to stop a job that is not running! Audio type: " + audioObject.name);
                return;
            }

            IEnumerator runningJob = (IEnumerator) _jobTable[audioObject];
            StopCoroutine(runningJob);
            _jobTable.Remove(audioObject);
        }
        
        // Give the audio track a audio source to play the corresponding audio clip of the audio job
        private IEnumerator RunAudioJob(AudioJob audioJob)
        {
            // Get the audio track
            AudioTrackSo track = audioJob.AudioObject.PlayingTrack;
            
            if (track.MultiPlayingTrack)
            {
                track.RegisterAudioSource(audioSourcePool.GetSource());
            }
            else
            {
                if (track.AudioSource == null)
                {
                    track.RegisterAudioSource(audioSourcePool.GetSource());
                }
            }
            track.RegisterAudioClip(audioJob.AudioObject.AudioClip);   
            
            // Deal with delay
            yield return new WaitForSeconds(audioJob.DelayTime);
            
            switch (audioJob.Action)
            {
                case AudioJobAction.START:
                    track.AudioSource.Play();
                    track.PlayingObject = audioJob.AudioObject;
                    break;
                case AudioJobAction.STOP:
                    if (!audioJob.Fade)
                    {
                        track.AudioSource.Stop();
                        track.PlayingObject = default;
                    }
                    break;
                case AudioJobAction.RESTART:
                    track.AudioSource.Stop();
                    track.AudioSource.Play();
                    track.PlayingObject = audioJob.AudioObject;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (audioJob.Fade)
            {
                float initialValue = audioJob.Action == AudioJobAction.STOP ? 1.0f : 0.0f;
                float endValue = 1.0f - initialValue;
                float timer = 0.0f;

                while (timer < audioJob.FadeTime)
                {
                    track.AudioSource.volume = Mathf.Lerp(initialValue, endValue, timer / audioJob.FadeTime);
                    timer += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }

                if (audioJob.Action == AudioJobAction.STOP)
                {
                    track.AudioSource.Stop();
                    track.PlayingObject = default;
                }
            }
            
            _jobTable.Remove(audioJob.AudioObject.name);
            //Debug.Log("Job Count: " + _jobTable.Count);
        }

        protected override void InitManager()
        {
            _jobTable = new Hashtable();
            audioSourcePool = new AudioSourcePool(audioSourcePoolPrefab, audioSourcePoolParent);
            GenerateAudioSourcesBasedOnTrack();
            playAudioEvent.Register(PlayAudio);
        }
        
        private void GenerateAudioSourcesBasedOnTrack()
        {
            foreach (var audioTrack in audioTracks)
            {
                audioTrack.RegisterAudioSource(audioSourcePool.GetSource());
                audioTrack.PlayingObject = null;
            }
        }
        
        private void Dispose()
        {
            foreach (DictionaryEntry entry in _jobTable)
            {
                IEnumerator job = (IEnumerator) entry.Value;
                StopCoroutine(job);
            }
            playAudioEvent.Unregister(PlayAudio);
        }

        #endregion
    }
}