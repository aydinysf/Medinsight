import { useEffect, useRef, useState } from 'react';
import { useHizirChat, useSendHizirMessage } from '../api';

/** Hızır sohbeti (ADR-018): vaka kapsamlı, guardrail'li — tanı/doz vermez, doktora yönlendirir. */
export function HizirChatTab({ caseId }: { caseId: string }) {
  const chat = useHizirChat(caseId);
  const send = useSendHizirMessage(caseId);
  const [message, setMessage] = useState('');
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [chat.data?.length, send.isPending]);

  const onSend = () => {
    const text = message.trim();
    if (!text || send.isPending) return;
    send.mutate(text, { onSuccess: () => setMessage('') });
  };

  return (
    <div className="flex h-[28rem] flex-col rounded-xl border border-gray-200 bg-white shadow-sm">
      <div className="flex-1 space-y-3 overflow-y-auto p-4">
        {chat.isLoading && <p className="text-sm text-gray-500">Yükleniyor…</p>}

        {chat.data?.length === 0 && !send.isPending && (
          <div className="flex items-start gap-3">
            <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-brand-600 text-sm font-semibold text-white">H</span>
            <div className="rounded-2xl rounded-tl-sm bg-brand-50 px-4 py-2 text-sm text-brand-900">
              Merhaba, ben Hızır. Vakan ve belgelerinle ilgili soruların varsa buradayım.
              Unutma: tanı koymam, tedavi kararı vermem — o kararlar doktorunundur.
            </div>
          </div>
        )}

        {chat.data?.map((m) =>
          m.isFromHizir ? (
            <div key={m.id} className="flex items-start gap-3">
              <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-brand-600 text-sm font-semibold text-white">H</span>
              <div>
                <div className="max-w-md whitespace-pre-wrap rounded-2xl rounded-tl-sm bg-brand-50 px-4 py-2 text-sm text-brand-900">
                  {m.content}
                </div>
                <p className="mt-1 text-xs text-gray-400">{new Date(m.createdAtUtc).toLocaleTimeString('tr-TR')}</p>
              </div>
            </div>
          ) : (
            <div key={m.id} className="flex justify-end">
              <div>
                <div className="max-w-md whitespace-pre-wrap rounded-2xl rounded-tr-sm bg-brand-600 px-4 py-2 text-sm text-white">
                  {m.content}
                </div>
                <p className="mt-1 text-right text-xs text-gray-400">{new Date(m.createdAtUtc).toLocaleTimeString('tr-TR')}</p>
              </div>
            </div>
          ),
        )}

        {send.isPending && (
          <div className="flex items-start gap-3">
            <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-brand-600 text-sm font-semibold text-white">H</span>
            <div className="rounded-2xl rounded-tl-sm bg-brand-50 px-4 py-2 text-sm text-gray-500">
              Hızır yazıyor…
            </div>
          </div>
        )}

        <div ref={bottomRef} />
      </div>

      {send.error && <p className="px-4 pb-1 text-sm text-red-600">{send.error.message}</p>}

      <div className="flex items-center gap-2 border-t border-gray-100 p-3">
        <input
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyDown={(e) => { if (e.key === 'Enter') onSend(); }}
          placeholder="Hızır'a sor… (örn. raporumda ne yazıyor?)"
          className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-600 focus:outline-none"
        />
        <button
          onClick={onSend}
          disabled={send.isPending || message.trim().length === 0}
          className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700 disabled:opacity-50"
        >
          Gönder
        </button>
      </div>

      <p className="border-t border-gray-100 px-4 py-2 text-xs text-gray-400">
        Hızır bir ön bilgilendirme asistanıdır; tanı ve tedavi kararları doktorunundur. Acil durumda 112'yi arayın.
      </p>
    </div>
  );
}
