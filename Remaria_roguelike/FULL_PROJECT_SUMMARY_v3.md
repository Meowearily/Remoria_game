# Полный обзор проекта: Remaria Roguelike (v3) — Код и Механики

Этот документ представляет собой технический разбор ключевых систем игры с примерами кода. Здесь объясняется, **как** реализованы основные механики и **почему** выбраны именно такие решения.

---

## 1. Универсальная система здоровья (Health System)
Система здоровья реализована как модульный компонент, который можно прикрепить к любому объекту (Игроку, Врагу, бочке).

### Код: `Health.cs`
```csharp
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action OnDied;

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        _currentHealth -= amount;
        _currentHealth = Mathf.Max(_currentHealth, 0f);

        // Оповещение подписчиков (например, UI)
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth <= 0f) Die();
    }
}
```
**Зачем это нужно?**
*   **Слабая связь (Decoupling):** Здоровье не знает о существовании UI. Оно просто "кричит" в мир: "Моё здоровье изменилось!". UI слушает этот крик и обновляет полоску. Это позволяет легко менять интерфейс, не трогая логику боя.
*   **Интерфейс `IDamageable`:** Позволяет любой пуле или мечу наносить урон любому объекту, не зная, кто это — просто вызвав `TakeDamage()`.

---

## 2. Процедурная генерация уровней (Hybrid Level Generation)
Генератор совмещает фиксированные комнаты и динамические коридоры, создавая уникальный лабиринт при каждом запуске.

### Код: `HybridLevelGenerator.cs` (Фазы генерации)
```csharp
public void Generate()
{
    Clear();
    GenerateRooms();      // Фаза 1: Размещение комнат в сетке DungeonGrid
    GenerateCorridors();  // Фаза 2: Соединение комнат через алгоритм A*
    GenerateWalls();      // Фаза 3: Окружение пола стенами
    BuildLevel();         // Фаза 4: Расстановка 3D-префабов на сцене
    DistributeContent();  // Фаза 5: Спавн Игрока, Врагов и Лута
}
```
**Зачем это нужно?**
*   **Реиграбельность:** Игрок никогда не увидит один и тот же уровень дважды.
*   **A* Pathfinding:** Используется не для навигации врагов, а для "прокопки" тоннелей между комнатами. Это гарантирует, что из любой точки уровня можно дойти до выхода.

---

## 3. Искусственный интеллект (FSM - Finite State Machine)
Враги управляются конечным автоматом. Они находятся в одном из состояний: `Idle`, `Patrol`, `Chase`, `Attack`.

### Код: `EnemyAI.cs` (Логика патрулирования)
```csharp
public void SetHomeRoom(RoomData room)
{
    _homeRoom = room;
    _patrolPoints.Clear();
    // ИИ сам выбирает случайные точки внутри своей комнаты для патрулирования
    List<Vector2Int> tiles = new List<Vector2Int>(_homeRoom.Tiles);
    for (int i = 0; i < maxPatrolPoints; i++) {
        int index = Random.Range(0, tiles.Count);
        Vector2Int tile = tiles[index]; // Берем случайную плитку
        _patrolPoints.Add(new Vector3(tile.x * TileSize, 0, tile.y * TileSize));
        tiles.RemoveAt(index);
    }
}
```
**Зачем это нужно?**
*   **Автономность:** Разработчику не нужно вручную расставлять точки пути для каждого монстра. Генератор дает врагу комнату, и монстр сам решает, где он будет ходить.
*   **Оптимизация (Raycast):** Проверка видимости игрока (`CanSeePlayer`) выполняется не каждый кадр, а через интервал (например, 0.2 сек), что экономит ресурсы процессора.

---

## 4. Боевая система и Lock-On (Combat System)
Игрок может сражаться в ближнем и дальнем бою, а также фокусировать камеру на цели.

### Код: `PlayerCombat.cs` (Механика Lock-On)
```csharp
private void ToggleLockOn()
{
    // Бросаем луч из камеры в точку курсора
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out hit, lockOnMaxDistance, lockOnLayers))
    {
        Health health = hit.collider.GetComponent<Health>();
        if (health != null && !health.IsDead) {
            _lockOnTarget = hit.collider.transform;
            CreateLockOnIndicator(); // Желтый алмаз над головой врага
        }
    }
}
```
**Зачем это нужно?**
*   **Удобство управления:** В Roguelike играх с видом сверху/сбоку часто сложно целиться. Lock-On автоматически поворачивает персонажа к врагу, позволяя игроку сосредоточиться на уклонении.
*   **OverlapSphere (Ближний бой):** Вместо сложных триггеров используется мгновенная проверка сферы перед игроком. Это работает очень быстро и надежно определяет попадания.

---

## 5. Мета-прогрессия и Сохранение (Save System)
Игра сохраняет прогресс в JSON файл, позволяя игроку покупать постоянные улучшения.

### Код: `SaveManager.cs` (Упрощенно)
```csharp
public void Save()
{
    string json = JsonUtility.ToJson(_currentSaveData);
    File.WriteAllText(SavePath, json);
}
```
**Зачем это нужно?**
*   **Развитие игрока:** Даже если игрок проиграл забег, собранные "Осколки" сохраняются. Это мотивирует играть дальше, так как персонаж становится сильнее с каждой попыткой (через скрипт `StatApplier`).

---

## Резюме архитектуры
Проект построен на принципе **"События и Компоненты"**. Вместо того чтобы создавать один огромный скрипт "Враг", мы собираем его из:
1. `Health` (Здоровье)
2. `EnemyAI` (Мозги)
3. `Rigidbody` (Физика)
4. `EnemyController` (Визуал)

Это позволяет легко создавать новых врагов, просто меняя настройки в этих компонентах.
