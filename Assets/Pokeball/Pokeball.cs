using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(Animator))]
public class Pokeball : MonoBehaviour
{
    public float DebugCaptureRate = 0.5f;
    public float RotationSpeed = 500f;
    public float PositionSpeed = 500f;
    public float PokemonPositionSpeed = 500f;
    public float PokemonScaleSpeed = 500f;
    public float BounceDistance = 0.5f;
    public ParticleSystem CaptureParticles;
    public ParticleSystem StarsParticles;
    public ParticleSystem BrokeParticles;
    public MeshRenderer[] PokeballRenderers;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private Quaternion _rotationGoal;
    private Vector3 _positionGoal;
    private Transform _capturedPokemon = null;
    private Vector3 _capturedPokemonScale;
    private Vector3 _capturedPokemonPosition;
    private Quaternion _capturedPokemonRotation;
    private bool _checkingCapture = false;
    private int _remainingWiggle = 0;
    private bool _brokeAnimation = false;
    private bool _isCaptured = false;
    private float _totalDistance = 0f;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_capturedPokemon == null)
            return;
        if (_checkingCapture == false && CatchPokemonInAir() && _rigidbody.isKinematic)
        {
            _capturedPokemon.localScale = Vector3.zero;
            _capturedPokemon.transform.parent = transform;
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;
        }
        if (_brokeAnimation == true)
        {
            bool brokeAnimationFinished = PokemonBroke();
            if (brokeAnimationFinished)
            {
                Destroy(gameObject);
            }
        }
    }

    private bool CatchPokemonInAir()
    {
        bool animationInAirEnded = true;
        if (transform.rotation != _rotationGoal)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _rotationGoal, RotationSpeed * Time.deltaTime);
            animationInAirEnded = false;
        }
        if (transform.position != _positionGoal)
        {
            transform.position = Vector3.MoveTowards(transform.position, _positionGoal, PositionSpeed * Time.deltaTime);
            animationInAirEnded = false;
        }
        if (_capturedPokemon.position != transform.position)
        {
            _capturedPokemon.position = Vector3.MoveTowards(_capturedPokemon.position, transform.position, PokemonPositionSpeed * Time.deltaTime);
            _capturedPokemon.localScale = _capturedPokemonScale * Mathf.Clamp(((_totalDistance - Vector3.Distance(_capturedPokemonPosition, _capturedPokemon.position)) / _totalDistance), 0f, 1f);
            animationInAirEnded = false;
        }
        return animationInAirEnded;
    }

    private bool PokemonBroke()
    {
        bool animationBrokeEnded = true;
        if (_capturedPokemon.position != _capturedPokemonPosition)
        {
            _capturedPokemon.position = Vector3.MoveTowards(_capturedPokemon.position, _capturedPokemonPosition, PokemonPositionSpeed * 8 * Time.deltaTime);
            animationBrokeEnded = false;
        }
        if (_capturedPokemon.localScale != _capturedPokemonScale)
        {
            _capturedPokemon.localScale = Vector3.MoveTowards(_capturedPokemon.localScale, _capturedPokemonScale, PokemonPositionSpeed * 8 * Time.deltaTime);
            animationBrokeEnded = false;
        }
        return animationBrokeEnded;
    }

    private void StartPokemonCapture(Transform pokemon)
    {
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _rotationGoal = Quaternion.LookRotation((transform.position - pokemon.position).normalized);
        _positionGoal = transform.position - (pokemon.position - transform.position).normalized * BounceDistance;
        _animator.SetTrigger("Catch");
        _capturedPokemon = pokemon;
        _capturedPokemonPosition = _capturedPokemon.position;
        _capturedPokemonScale = _capturedPokemon.localScale;
        _capturedPokemonRotation = _capturedPokemon.rotation;
        _totalDistance = Vector3.Distance(_positionGoal, _capturedPokemonPosition);
    }

    private void CheckIfCaught()
    {
        float captureChance = UnityEngine.Random.Range(0f, 1f);
        float wigglesDiv = captureChance / (1 - DebugCaptureRate);
        Debug.Log($"CapRate = {DebugCaptureRate} Inverse = {1 / DebugCaptureRate} Wiggles = {wigglesDiv} CaptureChance = {captureChance}");

        if (wigglesDiv < 0.25f)
            _remainingWiggle = 0;
        else if (wigglesDiv < 0.5f)
            _remainingWiggle = 1;
        else if (wigglesDiv < 0.75f)
            _remainingWiggle = 2;
        else
            _remainingWiggle = 3;
        _isCaptured = wigglesDiv > 1;
        Debug.Log(_remainingWiggle);
        OnWiggleEnd();
    }

    private IEnumerator WaitBeforeAction(Action action)
    {
        yield return new WaitForSeconds(1);
        action.Invoke();
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (_capturedPokemon == null && other.gameObject.layer == LayerMask.NameToLayer("Pokemon"))
        {
            StartPokemonCapture(other.transform);
            Physics.IgnoreCollision(other.collider, GetComponent<Collider>());
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (_capturedPokemon)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
                _checkingCapture = true;
                transform.up = other.contacts[0].normal;

                StartCoroutine(WaitBeforeAction(() => {
                    CheckIfCaught();
                }));
            }
            else
                Destroy(gameObject);
        }
    }

    private void OnWiggleEnd()
    {
        if (_remainingWiggle > 0)
        {
            _remainingWiggle--;
            _animator.SetTrigger("Wiggle");
            return;
        }
        if (_isCaptured)
        {
            Debug.Log("Pokemon Captured !");
            StarsParticles.Play();
        }
        else
        {
            Debug.Log("Oh no, the pokemon broke free !");
            _capturedPokemon.transform.parent = null;
            foreach (var renderer in PokeballRenderers)
                renderer.enabled = false;
            // _capturedPokemon.localScale = _capturedPokemonScale;
            // _capturedPokemon.position = _capturedPokemonPosition;
            // _capturedPokemon = null;
            BrokeParticles.transform.parent = null;
            _capturedPokemon.rotation = _capturedPokemonRotation;
            BrokeParticles.Play();
            _brokeAnimation = true;
        }

    }

    private void PlayCaptureParticles()
    {
        CaptureParticles.Play();
    }
    
    private void StopCaptureParticles()
    {
        CaptureParticles.Stop();
    }
}
