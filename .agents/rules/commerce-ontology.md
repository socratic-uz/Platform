# Socratic Commerce Ontology & Perspective-Driven Architecture

## 1. Канонический принцип: Три перспективы взаимодействия

При проектировании любых API, контрактов Protobuf, Kafka топиков, CQRS команд/запросов и UI экранов строго разделяй **три перспективы**:

### Таблица 1. Три перспективы коммерческого взаимодействия

| # | Покупатель (`Customer Perspective`) | Продавец (`Seller Perspective`) | Наблюдатель (`Interaction Perspective`) |
| :-: | :--- | :--- | :--- |
| **1** | `Buying` — покупка | `Selling` — продажа | `Trading` — торговля |
| **2** | `Shopping` — выбор и поиск | `Marketing` — продвижение | `Commerce` — коммерческий обмен |
| **3** | `Ordering` — оформление заказа | `Order Processing` — обработка заказа | `Order Transaction` — транзакция заказа |
| **4** | `Paying` — оплата | `Payment Collection` — приём оплаты | `Payment Transaction` — платёжная операция |
| **5** | `Receiving` — получение | `Shipping` — отправка | `Delivery` — доставка |
| **6** | `Consuming` — потребление | `Providing` — предоставление | `Service Delivery` — оказание услуги |
| **7** | `Booking` — бронирование | `Offering` — предложение | `Reservation` — резервирование |
| **8** | `Renting` — аренда | `Leasing` — предоставление в аренду | `Rental Transaction` — арендная сделка |
| **9** | `Reviewing` — оценка | `Reputation Management` — управление репутацией | `Feedback Exchange` — обмен обратной связью |
| **10** | `Returning` — возврат | `Returns Management` — обработка возвратов | `Return Transaction` — операция возврата |
| **11** | `Referring` — рекомендация | `Referral Marketing` — стимулирование рекомендаций | `Referral` — рекомендация между участниками |

### Зоны применения перспектив:
1. **Покупатель (`Customer Perspective`)**:
   - **Глагольная форма (Active Gerund)**: Клиентский фронтенд (`Commerce`, `Web.UI`, мобильные приложения), внешние клиентские gRPC методы (`ShoppingService`, `OrderingService`, `PayingService`).
2. **Продавец (`Seller Perspective`)**:
   - **Процессная форма (Operations / Management)**: Кассовые узлы (`PointOfSale`), киоски (`Kiosk`), кухонные дисплеи (`KDS`), портал мерчанта (`Portal/Organization`), управление складом и фискализацией.
3. **Наблюдатель / Сделка (`Interaction / Observer Perspective`)**:
   - **Субстантивная форма (Domain Event / Transaction / Ledger)**: Топики Apache Kafka, сущности ScyllaDB, Saga Orchestrator, Outbox Pattern, журналы аудита и взаимных расчетов (`Settlement`).

---

## 2. 14 Этапов жизненного цикла сделки

Все коммерческие процессы Socratic маппятся на 14 фундаментальных этапов:

| Этап | Покупатель (`Customer`) | Продавец (`Seller`) | Наблюдатель (`Interaction`) |
| :--- | :--- | :--- | :--- |
| **1. Рынок** | `Demand` (спрос) | `Supply` (предложение) | `Market` (рынок) |
| **2. Поиск** | `Discovery` (обнаружение) | `Promotion` (продвижение) | `Exposure` (ознакомление) |
| **3. Выбор** | `Shopping` (выбор) | `Merchandising` (выкладка) | `Selection` (выбор) |
| **4. Оценка** | `Evaluation` (оценка) | `Positioning` (позиционирование) | `Comparison` (сравнение) |
| **5. Цена** | `Price Acceptance` (принятие цены) | `Pricing` (ценообразование) | `Price Formation` (формирование цены) |
| **6. Сделка** | `Buying` (покупка) | `Selling` (продажа) | `Trading` (торговля) |
| **7. Оформление** | `Ordering` (заказ) | `Order Processing` (обработка) | `Transaction` (транзакция) |
| **8. Оплата** | `Paying` (оплата) | `Payment Acceptance` (приём) | `Settlement` (расчёт) |
| **9. Подготовка** | `Waiting` (ожидание) | `Fulfillment` (исполнение) | `Order Fulfillment` (исполнение заказа) |
| **10. Передача** | `Receiving` (получение) | `Shipping` (отправка) | `Delivery` (доставка) |
| **11. Потребление** | `Consumption` (потребление) | `Service Provision` (предоставление) | `Service Delivery` (оказание услуги) |
| **12. Оценка опыта** | `Reviewing` (отзыв) | `Reputation Management` (управление) | `Feedback` (обратная связь) |
| **13. Возврат** | `Returning` (возврат) | `Returns Processing` (обработка) | `Reverse Transaction` (обратная транзакция) |
| **14. Повтор** | `Repurchasing` (повтор) | `Retention` (удержание) | `Repeat Commerce` (повторная коммерция) |

---

## 3. Правила нейминга в кодовой базе

1. **Не смешивать перспективы**:
   - Запрещено называть клиентский эндпоинт `ProcessOrder` — со стороны покупателя это `PlaceOrder` / `SubmitOrder` (контекст `Ordering`).
   - Запрещено называть бэк-офисный процесс кухни/склада `Waiting` — для мерчанта это `OrderFulfillment` или `PrepareOrder`.
   - Запрещено называть событие в Kafka `UserPaid` — в шине событий это `PaymentSettled` или `PaymentTransactionCompleted` с указанием сторон и реквизитов.
2. **Интеграция с 28 UX-режимами (`ProductUxMode`)**:
   - 28 коммерческих фич каталога определяют специфику этапов 3, 7, 9, 10 и 11 (например, для режима `Rental` этапы 7–11 транслируются в `Renting` ➔ `Leasing` ➔ `Rental Transaction`).

