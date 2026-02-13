using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DayAndNight.Core;

namespace DayAndNight.Core
{
    /// <summary>
    /// 音频管理器，负责游戏中所有音频的播放和控制
    /// 包括背景音乐、音效、3D音效等
    /// </summary>
    public class AudioManager : BaseManager<AudioManager>
    {
        #region 常量

        /// <summary>
        /// 音频设置保存键名
        /// </summary>
        private const string MUSIC_VOLUME_KEY = "Audio_MusicVolume";
        private const string SFX_VOLUME_KEY = "Audio_SFXVolume";
        private const string VOICE_VOLUME_KEY = "Audio_VoiceVolume";
        private const string IS_MUTED_KEY = "Audio_IsMuted";

        #endregion

        #region 私有字段

        /// <summary>
        /// 音乐播放器组件
        /// </summary>
        [Header("音乐设置")]
        [SerializeField]
        private AudioSource _musicSource;

        /// <summary>
        /// 音效播放器组件池
        /// </summary>
        [Header("音效设置")]
        [SerializeField]
        private List<AudioSource> _sfxSources = new List<AudioSource>();

        /// <summary>
        /// 正在播放的音乐AudioSource（用于淡入淡出过渡）
        /// </summary>
        private AudioSource _currentMusicSource;

        /// <summary>
        /// 下一个音乐AudioSource（用于淡入淡出）
        /// </summary>
        private AudioSource _nextMusicSource;

        /// <summary>
        /// 音乐音量（0-1）
        /// </summary>
        private float _musicVolume = CoreConfig.DEFAULT_MUSIC_VOLUME;

        /// <summary>
        /// 音效音量（0-1）
        /// </summary>
        private float _sfxVolume = CoreConfig.DEFAULT_SFX_VOLUME;

        /// <summary>
        /// 语音音量（0-1）
        /// </summary>
        private float _voiceVolume = CoreConfig.DEFAULT_VOICE_VOLUME;

        /// <summary>
        /// 是否静音
        /// </summary>
        private bool _isMuted = false;

        /// <summary>
        /// 音乐淡入淡出协程
        /// </summary>
        private Coroutine _fadeCoroutine;

        /// <summary>
        /// 音乐播放列表
        /// </summary>
        private Dictionary<string, AudioClip> _musicClips = new Dictionary<string, AudioClip>();

        /// <summary>
        /// 音效播放列表
        /// </summary>
        private Dictionary<string, AudioClip> _sfxClips = new Dictionary<string, AudioClip>();

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取音乐音量
        /// </summary>
        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                UpdateMusicVolume();
                SaveAudioSettings();
            }
        }

        /// <summary>
        /// 获取音效音量
        /// </summary>
        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                UpdateSFXVolume();
                SaveAudioSettings();
            }
        }

        /// <summary>
        /// 获取语音音量
        /// </summary>
        public float VoiceVolume
        {
            get => _voiceVolume;
            set
            {
                _voiceVolume = Mathf.Clamp01(value);
                UpdateVoiceVolume();
                SaveAudioSettings();
            }
        }

        /// <summary>
        /// 获取是否静音
        /// </summary>
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                UpdateMuteState();
                SaveAudioSettings();
            }
        }

        /// <summary>
        /// 获取当前播放的音乐名称
        /// </summary>
        public string CurrentMusicName { get; private set; }

        /// <summary>
        /// 获取当前是否正在播放音乐
        /// </summary>
        public bool IsPlayingMusic => _musicSource != null && _musicSource.isPlaying;

        /// <summary>
        /// 获取当前音乐播放进度（0-1）
        /// </summary>
        public float MusicProgress => _musicSource != null ? _musicSource.time / _musicSource.clip.length : 0f;

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// 唤醒时初始化音频系统
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // 确保有足够的AudioSource组件
            EnsureAudioSources();
        }

        /// <summary>
        /// 销毁时清理
        /// </summary>
        protected override void OnDestroy()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            base.OnDestroy();
        }

        #endregion

        #region 保护虚方法

        /// <summary>
        /// 执行初始化逻辑
        /// </summary>
        protected override void OnInitialize()
        {
            Debug.Log("[AudioManager] 开始初始化...");

            // 加载保存的音频设置
            LoadAudioSettings();

            // 加载音频资源
            LoadAudioResources();

            // 初始化音乐播放器
            InitializeMusicPlayer();

            Debug.Log("[AudioManager] 初始化完成");
        }

        /// <summary>
        /// 执行关闭逻辑
        /// </summary>
        protected override void OnShutdown()
        {
            // 停止所有音乐
            StopMusic();

            // 停止所有音效
            StopAllSFX();

            // 保存设置
            SaveAudioSettings();

            Debug.Log("[AudioManager] 已关闭");
        }

        #endregion

        #region 音乐播放方法

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clipName">音乐资源名称</param>
        /// <param name="loop">是否循环播放</param>
        /// <param name="fadeIn">是否淡入播放</param>
        public void PlayMusic(string clipName, bool loop = true, bool fadeIn = false)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                Debug.LogWarning("[AudioManager] 播放音乐失败：clipName为空");
                return;
            }

            // 尝试从缓存或资源中获取音乐
            AudioClip clip = GetMusicClip(clipName);

            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] 未找到音乐资源: {clipName}");
                return;
            }

            // 如果需要淡入淡出
            if (fadeIn)
            {
                FadeToMusic(clipName, loop);
            }
            else
            {
                // 直接播放新音乐
                _musicSource.Stop();
                _musicSource.clip = clip;
                _musicSource.loop = loop;
                _musicSource.Play();

                CurrentMusicName = clipName;

                // 触发事件
                EventManager.Instance.TriggerEvent(CoreEvents.AUDIO_MUSIC_STARTED, new AudioEventArgs(clipName));

                Debug.Log($"[AudioManager] 开始播放音乐: {clipName}");
            }
        }

        /// <summary>
        /// 播放背景音乐（使用AudioClip）
        /// </summary>
        /// <param name="clip">音乐片段</param>
        /// <param name="loop">是否循环播放</param>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] 播放音乐失败：clip为null");
                return;
            }

            _musicSource.Stop();
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();

            CurrentMusicName = clip.name;

            // 触发事件
            EventManager.Instance.TriggerEvent(CoreEvents.AUDIO_MUSIC_STARTED, new AudioEventArgs(clip.name));

            Debug.Log($"[AudioManager] 开始播放音乐: {clip.name}");
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        /// <param name="fadeOut">是否淡出停止</param>
        public void StopMusic(bool fadeOut = false)
        {
            if (fadeOut)
            {
                StartCoroutine(FadeOutMusic());
            }
            else
            {
                _musicSource.Stop();
                _musicSource.clip = null;
                CurrentMusicName = null;

                // 触发事件
                EventManager.Instance.TriggerEvent(CoreEvents.AUDIO_MUSIC_ENDED, new AudioEventArgs(null));
            }
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public void PauseMusic()
        {
            _musicSource.Pause();

            Debug.Log("[AudioManager] 音乐已暂停");
        }

        /// <summary>
        /// 恢复背景音乐播放
        /// </summary>
        public void ResumeMusic()
        {
            _musicSource.UnPause();

            Debug.Log("[AudioManager] 音乐已恢复");
        }

        /// <summary>
        /// 音乐淡入淡出过渡到新音乐
        /// </summary>
        /// <param name="clipName">新音乐名称</param>
        /// <param name="loop">是否循环播放</param>
        /// <param name="fadeDuration">淡入淡出时间（秒）</param>
        public void FadeToMusic(string clipName, bool loop = true, float fadeDuration = CoreConfig.MUSIC_FADE_DURATION)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeToMusicRoutine(clipName, loop, fadeDuration));
        }

        /// <summary>
        /// 设置背景音乐音量和淡入淡出
        /// </summary>
        /// <param name="targetVolume">目标音量（0-1）</param>
        /// <param name="duration">过渡时间（秒）</param>
        public void SetMusicVolume(float targetVolume, float duration = 0.5f)
        {
            StartCoroutine(VolumeFade(_musicSource, targetVolume, duration));
        }

        #endregion

        #region 音效播放方法

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clipName">音效资源名称</param>
        /// <param name="volume">音量倍率（0-1）</param>
        public void PlaySFX(string clipName, float volume = 1f)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                Debug.LogWarning("[AudioManager] 播放音效失败：clipName为空");
                return;
            }

            AudioClip clip = GetSFXClip(clipName);

            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] 未找到音效资源: {clipName}");
                return;
            }

            // 查找可用的AudioSource
            AudioSource source = GetAvailableSFXSource();

            if (source != null)
            {
                source.PlayOneShot(clip, volume * _sfxVolume);

                // 触发事件
                EventManager.Instance.TriggerEvent(CoreEvents.AUDIO_SFX_PLAYED, new AudioEventArgs(clipName, volume));

                // 记录日志（调试模式）
                if (CoreConfig.ENABLE_DEBUG_MODE)
                {
                    Debug.Log($"[AudioManager] 播放音效: {clipName}, 音量: {volume}");
                }
            }
        }

        /// <summary>
        /// 播放音效（使用AudioClip）
        /// </summary>
        /// <param name="clip">音效片段</param>
        /// <param name="volume">音量倍率（0-1）</param>
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] 播放音效失败：clip为null");
                return;
            }

            AudioSource source = GetAvailableSFXSource();

            if (source != null)
            {
                source.PlayOneShot(clip, volume * _sfxVolume);

                // 触发事件
                EventManager.Instance.TriggerEvent(CoreEvents.AUDIO_SFX_PLAYED, new AudioEventArgs(clip.name, volume));

                Debug.Log($"[AudioManager] 播放音效: {clip.name}");
            }
        }

        /// <summary>
        /// 在指定位置播放3D音效
        /// </summary>
        /// <param name="clipName">音效资源名称</param>
        /// <param name="position">播放位置</param>
        /// <param name="volume">音量倍率（0-1）</param>
        /// <param name="spatialBlend">空间混合（0=2D, 1=3D）</param>
        public void PlaySFXAtPosition(string clipName, Vector3 position, float volume = 1f, float spatialBlend = 1f)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                Debug.LogWarning("[AudioManager] 播放3D音效失败：clipName为空");
                return;
            }

            AudioClip clip = GetSFXClip(clipName);

            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] 未找到3D音效资源: {clipName}");
                return;
            }

            // 创建临时AudioSource播放3D音效
            GameObject sfxObject = new GameObject($"3D_SFX_{clipName}");
            sfxObject.transform.position = position;

            AudioSource source = sfxObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = spatialBlend;
            source.volume = volume * _sfxVolume;
            source.Play();

            // 音效播放完成后销毁对象
            Destroy(sfxObject, clip.length + 0.1f);

            // 触发事件
            EventManager.Instance.TriggerEvent(CoreEvents.AUDIO_SFX_PLAYED, new AudioEventArgs(clipName, volume, true, position));

            Debug.Log($"[AudioManager] 播放3D音效: {clipName} at {position}");
        }

        /// <summary>
        /// 播放随机音效
        /// </summary>
        /// <param name="clipNames">音效名称数组</param>
        /// <param name="volume">音量倍率（0-1）</param>
        public void PlayRandomSFX(string[] clipNames, float volume = 1f)
        {
            if (clipNames == null || clipNames.Length == 0)
            {
                Debug.LogWarning("[AudioManager] 播放随机音效失败：clipNames为空");
                return;
            }

            string randomClip = clipNames[Random.Range(0, clipNames.Length)];
            PlaySFX(randomClip, volume);
        }

        /// <summary>
        /// 停止所有音效
        /// </summary>
        public void StopAllSFX()
        {
            foreach (var source in _sfxSources)
            {
                source.Stop();
            }

            Debug.Log("[AudioManager] 已停止所有音效");
        }

        /// <summary>
        /// 暂停所有音效
        /// </summary>
        public void PauseAllSFX()
        {
            foreach (var source in _sfxSources)
            {
                source.Pause();
            }

            Debug.Log("[AudioManager] 已暂停所有音效");
        }

        /// <summary>
        /// 恢复所有音效
        /// </summary>
        public void ResumeAllSFX()
        {
            foreach (var source in _sfxSources)
            {
                source.UnPause();
            }

            Debug.Log("[AudioManager] 已恢复所有音效");
        }

        #endregion

        #region 静音控制

        /// <summary>
        /// 切换静音状态
        /// </summary>
        public void ToggleMute()
        {
            IsMuted = !_isMuted;
        }

        /// <summary>
        /// 设置静音状态
        /// </summary>
        /// <param name="muted">是否静音</param>
        public void SetMute(bool muted)
        {
            IsMuted = muted;
        }

        #endregion

        #region 设置保存与加载

        /// <summary>
        /// 保存音频设置到PlayerPrefs
        /// </summary>
        public void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, _musicVolume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, _sfxVolume);
            PlayerPrefs.SetFloat(VOICE_VOLUME_KEY, _voiceVolume);
            PlayerPrefs.SetInt(IS_MUTED_KEY, _isMuted ? 1 : 0);
            PlayerPrefs.Save();

            if (CoreConfig.ENABLE_DEBUG_MODE)
            {
                Debug.Log("[AudioManager] 音频设置已保存");
            }
        }

        /// <summary>
        /// 从PlayerPrefs加载音频设置
        /// </summary>
        public void LoadAudioSettings()
        {
            // 确保有默认值
            if (!PlayerPrefs.HasKey(MUSIC_VOLUME_KEY))
            {
                PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, CoreConfig.DEFAULT_MUSIC_VOLUME);
            }
            if (!PlayerPrefs.HasKey(SFX_VOLUME_KEY))
            {
                PlayerPrefs.SetFloat(SFX_VOLUME_KEY, CoreConfig.DEFAULT_SFX_VOLUME);
            }
            if (!PlayerPrefs.HasKey(VOICE_VOLUME_KEY))
            {
                PlayerPrefs.SetFloat(VOICE_VOLUME_KEY, CoreConfig.DEFAULT_VOICE_VOLUME);
            }
            if (!PlayerPrefs.HasKey(IS_MUTED_KEY))
            {
                PlayerPrefs.SetInt(IS_MUTED_KEY, 0);
            }

            // 加载设置
            _musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY);
            _sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY);
            _voiceVolume = PlayerPrefs.GetFloat(VOICE_VOLUME_KEY);
            _isMuted = PlayerPrefs.GetInt(IS_MUTED_KEY) == 1;

            // 应用设置
            UpdateMusicVolume();
            UpdateSFXVolume();
            UpdateVoiceVolume();
            UpdateMuteState();

            if (CoreConfig.ENABLE_DEBUG_MODE)
            {
                Debug.Log($"[AudioManager] 音频设置已加载 - 音乐:{_musicVolume}, 音效:{_sfxVolume}, 静音:{_isMuted}");
            }
        }

        /// <summary>
        /// 重置音频设置为默认值
        /// </summary>
        public void ResetAudioSettings()
        {
            _musicVolume = CoreConfig.DEFAULT_MUSIC_VOLUME;
            _sfxVolume = CoreConfig.DEFAULT_SFX_VOLUME;
            _voiceVolume = CoreConfig.DEFAULT_VOICE_VOLUME;
            _isMuted = false;

            UpdateMusicVolume();
            UpdateSFXVolume();
            UpdateVoiceVolume();
            UpdateMuteState();
            SaveAudioSettings();

            Debug.Log("[AudioManager] 音频设置已重置为默认值");
        }

        #endregion

        #region 音频资源管理

        /// <summary>
        /// 预加载音乐资源
        /// </summary>
        /// <param name="clipNames">音乐名称数组</param>
        public void PreloadMusic(string[] clipNames)
        {
            foreach (string clipName in clipNames)
            {
                if (!_musicClips.ContainsKey(clipName))
                {
                    AudioClip clip = Resources.Load<AudioClip>($"Audio/Music/{clipName}");
                    if (clip != null)
                    {
                        _musicClips.Add(clipName, clip);
                        Debug.Log($"[AudioManager] 已预加载音乐: {clipName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[AudioManager] 未找到音乐资源: Audio/Music/{clipName}");
                    }
                }
            }
        }

        /// <summary>
        /// 预加载音效资源
        /// </summary>
        /// <param name="clipNames">音效名称数组</param>
        public void PreloadSFX(string[] clipNames)
        {
            foreach (string clipName in clipNames)
            {
                if (!_sfxClips.ContainsKey(clipName))
                {
                    AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{clipName}");
                    if (clip != null)
                    {
                        _sfxClips.Add(clipName, clip);
                        Debug.Log($"[AudioManager] 已预加载音效: {clipName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[AudioManager] 未找到音效资源: Audio/SFX/{clipName}");
                    }
                }
            }
        }

        /// <summary>
        /// 卸载未使用的音频资源
        /// </summary>
        public void UnloadUnusedAudio()
        {
            // 清理音乐缓存
            _musicClips.Clear();

            // 清理音效缓存
            _sfxClips.Clear();

            // 卸载未使用的资源
            Resources.UnloadUnusedAssets();

            Debug.Log("[AudioManager] 已卸载未使用的音频资源");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 确保有足够的AudioSource组件
        /// </summary>
        private void EnsureAudioSources()
        {
            // 主音乐播放器
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }

            // 确保有多个音效播放器
            while (_sfxSources.Count < 5)
            {
                AddSFXSource();
            }
        }

        /// <summary>
        /// 添加音效播放器
        /// </summary>
        private void AddSFXSource()
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            _sfxSources.Add(source);
        }

        /// <summary>
        /// 获取可用的音效播放器
        /// </summary>
        private AudioSource GetAvailableSFXSource()
        {
            // 查找未在播放的AudioSource
            foreach (var source in _sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // 如果所有源都在播放，添加一个新的
            AddSFXSource();
            return _sfxSources[_sfxSources.Count - 1];
        }

        /// <summary>
        /// 初始化音乐播放器
        /// </summary>
        private void InitializeMusicPlayer()
        {
            if (_musicSource != null)
            {
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }
        }

        /// <summary>
        /// 加载音频资源
        /// </summary>
        private void LoadAudioResources()
        {
            // 加载Resources文件夹中的音乐
            AudioClip[] musicClips = Resources.LoadAll<AudioClip>("Music");
            foreach (var clip in musicClips)
            {
                if (!_musicClips.ContainsKey(clip.name))
                {
                    _musicClips.Add(clip.name, clip);
                }
            }

            // 加载Resources文件夹中的音效
            AudioClip[] sfxClips = Resources.LoadAll<AudioClip>("SFX");
            foreach (var clip in sfxClips)
            {
                if (!_sfxClips.ContainsKey(clip.name))
                {
                    _sfxClips.Add(clip.name, clip);
                }
            }

            Debug.Log($"[AudioManager] 已加载 {musicClips.Length} 个音乐, {sfxClips.Length} 个音效");
        }

        /// <summary>
        /// 获取音乐片段
        /// </summary>
        private AudioClip GetMusicClip(string clipName)
        {
            // 先从缓存查找
            if (_musicClips.TryGetValue(clipName, out AudioClip cachedClip))
            {
                return cachedClip;
            }

            // 尝试从Resources加载
            AudioClip clip = Resources.Load<AudioClip>($"Music/{clipName}");
            if (clip != null)
            {
                _musicClips.Add(clipName, clip);
                return clip;
            }

            // 尝试从Resources根目录加载
            clip = Resources.Load<AudioClip>(clipName);
            if (clip != null)
            {
                _musicClips.Add(clipName, clip);
                return clip;
            }

            return null;
        }

        /// <summary>
        /// 获取音效片段
        /// </summary>
        private AudioClip GetSFXClip(string clipName)
        {
            // 先从缓存查找
            if (_sfxClips.TryGetValue(clipName, out AudioClip cachedClip))
            {
                return cachedClip;
            }

            // 尝试从Resources加载
            AudioClip clip = Resources.Load<AudioClip>($"SFX/{clipName}");
            if (clip != null)
            {
                _sfxClips.Add(clipName, clip);
                return clip;
            }

            // 尝试从Resources根目录加载
            clip = Resources.Load<AudioClip>(clipName);
            if (clip != null)
            {
                _sfxClips.Add(clipName, clip);
                return clip;
            }

            return null;
        }

        /// <summary>
        /// 更新音乐音量
        /// </summary>
        private void UpdateMusicVolume()
        {
            if (_musicSource != null)
            {
                _musicSource.volume = _isMuted ? 0f : _musicVolume;
            }
        }

        /// <summary>
        /// 更新音效音量
        /// </summary>
        private void UpdateSFXVolume()
        {
            foreach (var source in _sfxSources)
            {
                source.volume = _isMuted ? 0f : _sfxVolume;
            }
        }

        /// <summary>
        /// 更新语音音量（如果有语音AudioSource）
        /// </summary>
        private void UpdateVoiceVolume()
        {
            // 语音通常与其他系统集成，这里保留扩展性
        }

        /// <summary>
        /// 更新静音状态
        /// </summary>
        private void UpdateMuteState()
        {
            float effectiveVolume = _isMuted ? 0f : 1f;

            if (_musicSource != null)
            {
                _musicSource.volume = effectiveVolume * _musicVolume;
            }

            foreach (var source in _sfxSources)
            {
                source.volume = effectiveVolume * _sfxVolume;
            }
        }

        #endregion

        #region 协程方法

        /// <summary>
        /// 音乐淡入淡出过渡协程
        /// </summary>
        private IEnumerator FadeToMusicRoutine(string clipName, bool loop, float duration)
        {
            // 获取新音乐
            AudioClip newClip = GetMusicClip(clipName);

            if (newClip == null)
            {
                Debug.LogWarning($"[AudioManager] 淡入淡出失败：未找到音乐 {clipName}");
                yield break;
            }

            // 如果当前有音乐在播放，先淡出
            if (_musicSource.isPlaying)
            {
                yield return StartCoroutine(FadeOutMusic(duration));
            }

            // 播放新音乐
            _musicSource.clip = newClip;
            _musicSource.loop = loop;
            _musicSource.Play();

            CurrentMusicName = clipName;

            // 淡入新音乐
            yield return StartCoroutine(FadeInMusic(duration));

            // 触发事件
            EventManager.Instance.TriggerEvent(CoreEvents.AUDIO_MUSIC_STARTED, new AudioEventArgs(clipName));

            _fadeCoroutine = null;
        }

        /// <summary>
        /// 淡出音乐协程
        /// </summary>
        private IEnumerator FadeOutMusic(float duration = CoreConfig.MUSIC_FADE_DURATION)
        {
            float startVolume = _musicSource.volume;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            _musicSource.Stop();
            _musicSource.volume = startVolume;

            // 触发事件
            EventManager.Instance.TriggerEvent(CoreEvents.AUDIO_MUSIC_ENDED, new AudioEventArgs(CurrentMusicName));

            CurrentMusicName = null;
        }

        /// <summary>
        /// 淡入音乐协程
        /// </summary>
        private IEnumerator FadeInMusic(float duration = CoreConfig.MUSIC_FADE_DURATION)
        {
            float targetVolume = _isMuted ? 0f : _musicVolume;
            _musicSource.volume = 0f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            _musicSource.volume = targetVolume;
        }

        /// <summary>
        /// 音量渐变协程
        /// </summary>
        private IEnumerator VolumeFade(AudioSource source, float targetVolume, float duration)
        {
            float startVolume = source.volume;
            float effectiveTarget = _isMuted ? 0f : targetVolume;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, effectiveTarget, elapsed / duration);
                yield return null;
            }

            source.volume = effectiveTarget;
        }

        #endregion
    }

    #region 音频事件参数类

    /// <summary>
    /// 音频事件参数
    /// </summary>
    public class AudioEventArgs : System.EventArgs
    {
        /// <summary>
        /// 音频名称
        /// </summary>
        public string AudioName { get; }

        /// <summary>
        /// 音量
        /// </summary>
        public float Volume { get; }

        /// <summary>
        /// 是否为3D音效
        /// </summary>
        public bool Is3D { get; }

        /// <summary>
        /// 播放位置（3D音效）
        /// </summary>
        public Vector3 Position { get; }

        public AudioEventArgs(string audioName, float volume = 1f, bool is3D = false, Vector3 position = default)
        {
            AudioName = audioName;
            Volume = volume;
            Is3D = is3D;
            Position = position;
        }
    }

    #endregion
}
