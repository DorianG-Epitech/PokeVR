using System;
using System.Collections;
using UnityEngine.AI;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(Animator))]
public class Pokeball : NetworkBehaviour
{
    [SyncVar] public string ownerId;
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
    [SyncVar]
    private Transform _capturedPokemon = null;
    [SyncVar]
    private Vector3 _capturedPokemonScale;
    [SyncVar]
    private Vector3 _capturedPokemonPosition;
    [SyncVar]
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
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        if (_brokeAnimation == true)
        {
            bool brokeAnimationFinished = PokemonBroke();
            if (brokeAnimationFinished)
            {
                if (_capturedPokemon.GetComponentInChildren<NavMeshAgent>())
                {
                    _capturedPokemon.GetComponentInChildren<NavMeshAgent>().enabled = true;
                    _capturedPokemon.GetComponentInChildren<NavMeshAgent>().isStopped = false;
                }
                DestroyPokeball();
            }
        }
    }

    [Command]
    private void MovePokemonServer(GameObject pokemon, Vector3 capturedPokemonScale, Vector3 capturedPokemonPosition, float totalDistance, float pokemonPositionSpeed)
    {
        // _capturedPokemon.position = Vector3.MoveTowards(_capturedPokemon.position, transform.position, PokemonPositionSpeed * Time.deltaTime);
        // _capturedPokemon.localScale = _capturedPokemonScale * Mathf.Clamp(((_totalDistance - Vector3.Distance(_capturedPokemonPosition, _capturedPokemon.position)) / _totalDistance), 0f, 1f);
        pokemon.transform.position = Vector3.MoveTowards(pokemon.transform.position, transform.position, pokemonPositionSpeed * Time.deltaTime);
        pokemon.transform.localScale = capturedPokemonScale * Mathf.Clamp(((totalDistance - Vector3.Distance(capturedPokemonPosition, pokemon.transform.position)) / totalDistance), 0f, 1f);
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
            MovePokemonServer(_capturedPokemon.gameObject, _capturedPokemonScale, _capturedPokemonPosition, _totalDistance, PokemonPositionSpeed);
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

    private Transform GetRootParent(Transform pokemon)
    {
        
        if (pokemon.parent == null)
            return pokemon;
        Transform parent = pokemon.parent;

        while (parent != null)
        {
            pokemon = parent;
            parent = pokemon.parent;
        }
        return pokemon;
    }

    private void DeactivateCollisions(Transform pokemon)
    {
        foreach (var collider in pokemon.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(collider, GetComponent<Collider>());
        }
    }

    [Command]
    private void StartPokemonCaptureServer(GameObject pokemon)
    {
        if (pokemon && pokemon.GetComponentInChildren<NavMeshAgent>()) 
        {
            pokemon.GetComponentInChildren<NavMeshAgent>().isStopped = true;
            pokemon.GetComponentInChildren<NavMeshAgent>().enabled = false;
        }
    }

    private void StartPokemonCapture(Transform basePokemon)
    {
        Transform pokemon = GetRootParent(basePokemon);
        DeactivateCollisions(pokemon);
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        GetComponent<NetworkAnimator>().SetTrigger("Catch");
        // _animator.SetTrigger("Catch");
        _rotationGoal = Quaternion.LookRotation((transform.position - pokemon.position).normalized);
        _positionGoal = transform.position - (pokemon.position - transform.position).normalized * BounceDistance;
        _capturedPokemon = pokemon;
        _capturedPokemonPosition = _capturedPokemon.position;
        _capturedPokemonScale = _capturedPokemon.localScale;
        _capturedPokemonRotation = _capturedPokemon.rotation;
        _totalDistance = Vector3.Distance(_positionGoal, _capturedPokemonPosition);
        StartPokemonCaptureServer(_capturedPokemon.gameObject);
    }

    private void CheckIfCaught()
    {
        float captureChance = UnityEngine.Random.Range(0f, 1f);
        float wigglesDiv = captureChance / (1 - DebugCaptureRate);

        if (wigglesDiv < 0.25f)
            _remainingWiggle = 0;
        else if (wigglesDiv < 0.5f)
            _remainingWiggle = 1;
        else if (wigglesDiv < 0.75f)
            _remainingWiggle = 2;
        else
            _remainingWiggle = 3;
        _isCaptured = wigglesDiv > 1;
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
            SoundManager.PlaySound(SoundManager.Sound.PokeballHit, other.transform.position);
            StartPokemonCapture(other.transform);
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
            {
                DestroyPokeball();
            }
        }
    }

    private void OnWiggleEnd()
    {
        if (_remainingWiggle > 0)
        {
            _remainingWiggle--;
            _animator.SetTrigger("Wiggle");
            SoundManager.PlaySound(SoundManager.Sound.PokeballWiggle, _capturedPokemon.transform.position);
            return;
        }
        if (_isCaptured)
        {
            Pokemon poke = _capturedPokemon.GetComponentInChildren<Pokemon>();
            PokemonData data = poke.GetData();
            StarsParticles.transform.parent = null;
            Debug.LogError("Capturemanager: " + GameObject.FindObjectOfType<CaptureManager>());
            if (GameObject.FindObjectOfType<CaptureManager>())
                GameObject.FindObjectOfType<CaptureManager>().AddPokemon(ownerId, data, poke);
            // SoundManager.PlaySound(SoundManager.Sound.PokemonCatch, capturedPokemon.transform.position);
            Destroy(_capturedPokemon.gameObject);
            DestroyPokeball();
            StarsParticles.Play();
        }
        else
        {
            _capturedPokemon.transform.parent = null;
            foreach (var renderer in PokeballRenderers)
                renderer.enabled = false;
            BrokeParticles.transform.parent = null;
            _capturedPokemon.rotation = _capturedPokemonRotation;
            BrokeParticles.Play();
            _brokeAnimation = true;
        }

    }

    private void DestroyPokeball()
    {
        DestroyPokeballServer();
    }

    [Command]
    private void DestroyPokeballServer()
    {
        NetworkServer.Destroy(gameObject);
        Destroy(gameObject);
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
