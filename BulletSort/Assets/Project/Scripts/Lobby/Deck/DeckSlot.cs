using System;
using InGame.Sort.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Deck
{
    // 편성 슬롯 1칸 — Empty_BG(빈 칸) / DeckCard(초상화) / Piece_Image(기물 탄피)를 토글.
    // 그림자(Shadow)는 항상 켜진 받침이라 토글 대상 아님 — Piece_Image만 끄고 켬.
    // 빈 칸: Empty ON·카드 OFF·기물이미지 OFF / 편성: Empty OFF·카드 ON+Setup·기물이미지 ON+스프라이트.
    // 6칸 고정이라 미리 두고 토글(동적 스폰 아님).
    // 작성자: 이성규
    public class DeckSlot : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _emptyBG;
        [SerializeField] private DeckCard _card;        // 미리 둔 카드 (기본 OFF)

        [Header("기물 비주얼 (탄피)")]
        [SerializeField] private Image _pieceImage;      // Piece_Image — 토글 + GetSprite 세팅
                                                         // (Shadow는 항상 ON이라 토글 안 함)

        public bool IsEmpty => PieceID == 0;
        public int PieceID { get; private set; }

        // 슬롯 탭(해제) 콜백 — 편성된 카드를 탭하면 호출. DeckBuilder가 등록.
        private Action<DeckSlot> _onTapCard;

        public void Init(Action<DeckSlot> onTapCard)
        {
            _onTapCard = onTapCard;
            SetEmpty();
        }

        // 빈 칸으로
        public void SetEmpty()
        {
            PieceID = 0;
            if (_emptyBG != null) _emptyBG.SetActive(true);
            if (_card != null) _card.gameObject.SetActive(false);
            if (_pieceImage != null) _pieceImage.gameObject.SetActive(false);
        }

        // PieceID로 편성 — 카드·기물이미지 켜고 비주얼 채움. 카드 탭하면 해제(_onTapCard).
        public void SetPiece(int pieceID)
        {
            PieceID = pieceID;
            if (_emptyBG != null) _emptyBG.SetActive(false);
            if (_card != null)
            {
                _card.gameObject.SetActive(true);
                _card.Setup(pieceID, _ => _onTapCard?.Invoke(this));
            }
            if (_pieceImage != null)
            {
                _pieceImage.gameObject.SetActive(true);
                _pieceImage.sprite = PieceQuery.GetSprite(pieceID);
            }
        }
    }
}